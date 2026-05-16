using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DragonGlare.Persistence
{
    public class SaveManager : MonoBehaviour
    {
        public const int SlotCount = 3;
        private const int SignedSaveVersion = 5;
        private const string SignatureSecret = "DragonGlareAlpha::SaveSeal::2026-04-09";

        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            Formatting = Formatting.Indented
        };

        private string saveRootDirectory = string.Empty;

        private void Awake()
        {
            EnsureSaveRootDirectory();
        }

        public string GetSlotPath(int slotNumber)
        {
            ValidateSlotNumber(slotNumber);
            return Path.Combine(EnsureSaveRootDirectory(), $"slot{slotNumber}.sav");
        }

        public bool TryLoadSlot(int slotNumber, out SaveData saveData)
        {
            saveData = null;
            ValidateSlotNumber(slotNumber);

            try
            {
                var path = GetSlotPath(slotNumber);
                if (!File.Exists(path))
                {
                    return false;
                }

                var encrypted = File.ReadAllBytes(path);
                var json = Decrypt(encrypted, slotNumber);
                saveData = JsonConvert.DeserializeObject<SaveData>(json, SerializerSettings);

                if (saveData == null) return false;
                if (saveData.SlotNumber != 0 && saveData.SlotNumber != slotNumber) return false;
                saveData.SlotNumber = slotNumber;

                return HasValidSignature(saveData);
            }
            catch
            {
                return false;
            }
        }

        public void SaveSlot(int slotNumber, SaveData saveData)
        {
            ValidateSlotNumber(slotNumber);
            Directory.CreateDirectory(EnsureSaveRootDirectory());

            saveData.Version = SaveData.CurrentVersion;
            saveData.SlotNumber = slotNumber;
            saveData.Signature = ComputeSignature(saveData);

            var json = JsonConvert.SerializeObject(saveData, SerializerSettings);
            var encrypted = Encrypt(json, slotNumber);

            var path = GetSlotPath(slotNumber);
            var tempPath = $"{path}.tmp";
            File.WriteAllBytes(tempPath, encrypted);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
        }

        public bool CopySlot(int sourceSlotNumber, int destinationSlotNumber)
        {
            ValidateSlotNumber(sourceSlotNumber);
            ValidateSlotNumber(destinationSlotNumber);
            if (sourceSlotNumber == destinationSlotNumber) return false;

            if (!TryLoadSlot(sourceSlotNumber, out var saveData) || saveData == null)
                return false;

            SaveSlot(destinationSlotNumber, saveData);
            return true;
        }

        public bool DeleteSlot(int slotNumber)
        {
            ValidateSlotNumber(slotNumber);
            var path = GetSlotPath(slotNumber);
            var deleted = false;
            if (File.Exists(path))
            {
                File.Delete(path);
                deleted = true;
            }

            var tempPath = $"{path}.tmp";
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return deleted;
        }

        public SaveSlotSummary[] GetSlotSummaries()
        {
            var summaries = new SaveSlotSummary[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                var slotNumber = i + 1;
                if (TryLoadSlot(slotNumber, out var saveData) && saveData != null)
                {
                    summaries[i] = new SaveSlotSummary
                    {
                        SlotNumber = slotNumber,
                        State = SaveSlotState.Occupied,
                        Name = saveData.Name,
                        Level = saveData.Level,
                        Gold = saveData.Gold,
                        CurrentFieldMap = saveData.CurrentFieldMap,
                        SavedAtLocal = saveData.SavedAtUtc.ToLocalTime()
                    };
                }
                else
                {
                    summaries[i] = new SaveSlotSummary
                    {
                        SlotNumber = slotNumber,
                        State = File.Exists(GetSlotPath(slotNumber)) ? SaveSlotState.Corrupted : SaveSlotState.Empty
                    };
                }
            }
            return summaries;
        }

        private static byte[] Encrypt(string plainText, int slotNumber)
        {
            var key = DeriveKey(slotNumber);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            return result;
        }

        private static string Decrypt(byte[] cipherData, int slotNumber)
        {
            var key = DeriveKey(slotNumber);
            using var aes = Aes.Create();
            aes.Key = key;
            var iv = new byte[aes.IV.Length];
            Buffer.BlockCopy(cipherData, 0, iv, 0, iv.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var encrypted = new byte[cipherData.Length - iv.Length];
            Buffer.BlockCopy(cipherData, iv.Length, encrypted, 0, encrypted.Length);
            var plainBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] DeriveKey(int slotNumber)
        {
            var password = $"DragonGlareAlpha::UnitySave::{slotNumber}";
            return ComputeSha256(Encoding.UTF8.GetBytes(password));
        }

        private static bool HasValidSignature(SaveData saveData)
        {
            if (saveData.Version < SignedSaveVersion && string.IsNullOrWhiteSpace(saveData.Signature))
                return true;
            if (string.IsNullOrWhiteSpace(saveData.Signature))
                return false;

            var expected = Convert.FromBase64String(ComputeSignature(saveData));
            var actual = Convert.FromBase64String(saveData.Signature);
            return FixedTimeEquals(actual, expected);
        }

        private static string ComputeSignature(SaveData saveData)
        {
            var originalSignature = saveData.Signature;
            string payload;
            try
            {
                saveData.Signature = string.Empty;
                payload = JsonConvert.SerializeObject(saveData, SerializerSettings);
            }
            finally
            {
                saveData.Signature = originalSignature;
            }

            var key = ComputeSha256(Encoding.UTF8.GetBytes(SignatureSecret));
            var hash = ComputeHmacSha256(key, Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }

        private static byte[] ComputeSha256(byte[] data)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(data);
        }

        private static byte[] ComputeHmacSha256(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }

        private static void ValidateSlotNumber(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > SlotCount)
                throw new ArgumentOutOfRangeException(nameof(slotNumber));
        }

        private string EnsureSaveRootDirectory()
        {
            if (string.IsNullOrEmpty(saveRootDirectory))
            {
                saveRootDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            }

            return saveRootDirectory;
        }
    }
}
