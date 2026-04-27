using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Persistence;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private void UpdateSaveSlotSelection()
    {
        if (saveSlotSelectionMode != SaveSlotSelectionMode.DeleteConfirm)
        {
            if (WasPressed(Keys.Up) || WasPressed(Keys.W))
            {
                saveSlotCursor = Math.Max(0, saveSlotCursor - 1);
            }
            else if (WasPressed(Keys.Down) || WasPressed(Keys.S))
            {
                saveSlotCursor = Math.Min(SaveService.SlotCount - 1, saveSlotCursor + 1);
            }
        }

        if (WasPressed(Keys.Escape))
        {
            CancelSaveSlotSelection();
            return;
        }

        if (!WasPressed(Keys.Enter))
        {
            return;
        }

        var selectedSlot = saveSlotCursor + 1;
        switch (saveSlotSelectionMode)
        {
            case SaveSlotSelectionMode.Load:
                LoadSelectedSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.CopySource:
                SelectCopySourceSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.CopyDestination:
                CopyToSelectedSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.DeleteSelect:
                SelectDeleteSlot(selectedSlot);
                return;
            case SaveSlotSelectionMode.DeleteConfirm:
                DeleteSelectedSlot();
                return;
            default:
                activeSaveSlot = selectedSlot;
                SaveGame();
                ChangeGameState(GameState.Field);
                return;
        }
    }

    private void LoadSelectedSlot(int selectedSlot)
    {
        if (TryLoadGame(selectedSlot))
        {
            ChangeGameState(GameState.Field);
            return;
        }

        var failureReason = saveService.LastFailureReason;
        RefreshSaveSlotSummaries();
        ShowTransientNotice(GetSaveLoadFailureMessage(failureReason));
        PlaySe(SoundEffect.Collision);
    }

    private void SelectCopySourceSlot(int selectedSlot)
    {
        if (!IsSaveSlotOccupied(selectedSlot))
        {
            ShowTransientNotice("うつすデータがありません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        dataOperationSourceSlot = selectedSlot;
        saveSlotSelectionMode = SaveSlotSelectionMode.CopyDestination;
        saveSlotCursor = GetDefaultCopyDestinationIndex(selectedSlot);
        ShowTransientNotice($"ぼうけんのしょ {selectedSlot} を どこへうつしますか？", 240);
        PlaySe(SoundEffect.Dialog);
    }

    private void CopyToSelectedSlot(int selectedSlot)
    {
        var sourceSlot = dataOperationSourceSlot;
        if (sourceSlot is < 1 or > SaveService.SlotCount)
        {
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            ShowTransientNotice("うつすデータを えらびなおしてください。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (selectedSlot == sourceSlot)
        {
            ShowTransientNotice("同じ枠へは うつせません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        bool copied;
        try
        {
            copied = saveService.CopySlot(sourceSlot, selectedSlot);
        }
        catch
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
            ShowTransientNotice("SAVE DATA ERROR / データを処理できません");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (copied)
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
            saveSlotCursor = selectedSlot - 1;
            ShowTransientNotice($"ぼうけんのしょ {sourceSlot} を {selectedSlot} にうつしました。", 240);
            PlaySe(SoundEffect.Dialog);
            return;
        }

        var failureReason = saveService.LastFailureReason;
        RefreshSaveSlotSummaries();
        dataOperationSourceSlot = 0;
        saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
        saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
        ShowTransientNotice(GetSaveLoadFailureMessage(failureReason));
        PlaySe(SoundEffect.Collision);
    }

    private void SelectDeleteSlot(int selectedSlot)
    {
        var summary = saveSlotSummaries.ElementAtOrDefault(selectedSlot - 1);
        if (summary is null || summary.State == SaveSlotState.Empty)
        {
            ShowTransientNotice("けすデータがありません。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        dataOperationSourceSlot = selectedSlot;
        saveSlotCursor = selectedSlot - 1;
        saveSlotSelectionMode = SaveSlotSelectionMode.DeleteConfirm;
        ShowTransientNotice($"ぼうけんのしょ {selectedSlot} を けしますか？", 240);
        PlaySe(SoundEffect.Dialog);
    }

    private void DeleteSelectedSlot()
    {
        var selectedSlot = dataOperationSourceSlot;
        if (selectedSlot is < 1 or > SaveService.SlotCount)
        {
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
            ShowTransientNotice("けすデータを えらびなおしてください。");
            PlaySe(SoundEffect.Collision);
            return;
        }

        bool deleted;
        try
        {
            deleted = saveService.DeleteSlot(selectedSlot);
        }
        catch
        {
            RefreshSaveSlotSummaries();
            dataOperationSourceSlot = 0;
            saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
            ShowTransientNotice("SAVE DATA ERROR / データを処理できません");
            PlaySe(SoundEffect.Collision);
            return;
        }

        if (activeSaveSlot == selectedSlot)
        {
            activeSaveSlot = 0;
        }

        RefreshSaveSlotSummaries();
        dataOperationSourceSlot = 0;
        saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
        saveSlotCursor = Math.Clamp(selectedSlot - 1, 0, SaveService.SlotCount - 1);
        ShowTransientNotice(deleted
            ? $"ぼうけんのしょ {selectedSlot} を けしました。"
            : "けすデータがありません。", 240);
        PlaySe(deleted ? SoundEffect.Dialog : SoundEffect.Collision);
    }

    private void CancelSaveSlotSelection()
    {
        switch (saveSlotSelectionMode)
        {
            case SaveSlotSelectionMode.Save:
                ChangeGameState(GameState.NameInput);
                return;
            case SaveSlotSelectionMode.CopyDestination:
                dataOperationSourceSlot = 0;
                saveSlotSelectionMode = SaveSlotSelectionMode.CopySource;
                saveSlotCursor = GetFirstOccupiedSaveSlotIndex();
                menuNotice = string.Empty;
                menuNoticeFrames = 0;
                return;
            case SaveSlotSelectionMode.DeleteConfirm:
                saveSlotSelectionMode = SaveSlotSelectionMode.DeleteSelect;
                saveSlotCursor = Math.Clamp(dataOperationSourceSlot - 1, 0, SaveService.SlotCount - 1);
                dataOperationSourceSlot = 0;
                menuNotice = string.Empty;
                menuNoticeFrames = 0;
                return;
            default:
                dataOperationSourceSlot = 0;
                ChangeGameState(GameState.ModeSelect);
                return;
        }
    }

    private bool IsSaveSlotOccupied(int slotNumber)
    {
        return saveSlotSummaries.ElementAtOrDefault(slotNumber - 1)?.State == SaveSlotState.Occupied;
    }

    private static string GetSaveLoadFailureMessage(SaveLoadFailureReason failureReason)
    {
        return failureReason switch
        {
            SaveLoadFailureReason.InvalidSignature => "SAVE DATA INVALID / セーブデータが改ざんされています",
            SaveLoadFailureReason.InvalidFormat => "SAVE DATA ERROR / セーブデータが壊れています",
            _ => "NO SAVE DATA / セーブデータがありません"
        };
    }

    private int GetDefaultCopyDestinationIndex(int sourceSlot)
    {
        for (var index = 0; index < saveSlotSummaries.Count; index++)
        {
            if (index + 1 != sourceSlot && saveSlotSummaries[index].State == SaveSlotState.Empty)
            {
                return index;
            }
        }

        return GetFirstDifferentSaveSlotIndex(sourceSlot);
    }

    private static int GetFirstDifferentSaveSlotIndex(int sourceSlot)
    {
        for (var slotNumber = 1; slotNumber <= SaveService.SlotCount; slotNumber++)
        {
            if (slotNumber != sourceSlot)
            {
                return slotNumber - 1;
            }
        }

        return 0;
    }
}
