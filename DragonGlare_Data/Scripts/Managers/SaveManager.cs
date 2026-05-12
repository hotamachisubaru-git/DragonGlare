using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DragonGlare.Persistence
{
    public class SaveManager : MonoBehaviour
    {
        public const int SlotCount = 3;
        private const int SignedSaveVersion = 5;
        private const string SignatureSecret = "DragonGlareAlpha::SaveSeal::2026-04-09";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private string saveRootDirectory;

        private void Awake()
        {
            saveRootDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        }

        public string GetSlotPath(int slotNumber)
        {
            ValidateSlotNumber(slotNumber);
            return Path.Combine(saveRootDirectory, $"slot{slotNumber}.sav");
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
                saveData = JsonSerializer.Deserialize<SaveData>(json, SerializerOptions);

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
            Directory.CreateDirectory(saveRootDirectory);

            saveData.Version = SaveData.CurrentVersion;
            saveData.SlotNumber = slotNumber;
            saveData.Signature = ComputeSignature(saveData);

            var json = JsonSerializer.Serialize(saveData, SerializerOptions);
            var encrypted = Encrypt(json, slotNumber);

            var path = GetSlotPath(slotNumber);
            var tempPath = $"{path}.tmp";
            File.WriteAllBytes(tempPath, encrypted);
            File.Move(tempPath, path, overwrite: true);
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
            return SHA256.HashData(Encoding.UTF8.GetBytes(password));
        }

        private static bool HasValidSignature(SaveData saveData)
        {
            if (saveData.Version < SignedSaveVersion && string.IsNullOrWhiteSpace(saveData.Signature))
                return true;
            if (string.IsNullOrWhiteSpace(saveData.Signature))
                return false;

            var expected = Convert.FromBase64String(ComputeSignature(saveData));
            var actual = Convert.FromBase64String(saveData.Signature);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static string ComputeSignature(SaveData saveData)
        {
            var payload = JsonSerializer.Serialize(saveData, SerializerOptions);
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(SignatureSecret));
            var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }

        private static void ValidateSlotNumber(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > SlotCount)
                throw new ArgumentOutOfRangeException(nameof(slotNumber));
        }
    }
}
