using DragonGlareAlpha.Data;
using DragonGlareAlpha.Domain;
using DragonGlareAlpha.Domain.Battle;
using DragonGlareAlpha.Domain.Items;
using DragonGlareAlpha.Domain.Player;

namespace DragonGlareAlpha;

public partial class DragonGlareAlpha
{
    private string GetBattleEncounterMessage(string enemyName)
    {
        return selectedLanguage == UiLanguage.English
            ? $"{enemyName} is watching you!"
            : $"{enemyName}はこちらをみている！";
    }

    private string GetBattleCommandPromptMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "What will you do?"
            : "どうする？";
    }

    private string GetBattleOpeningCommandMessage()
    {
        return currentEncounter is null
            ? GetBattleCommandPromptMessage()
            : $"{GetBattleEncounterMessage(GameContent.GetEnemyName(currentEncounter.Enemy, selectedLanguage))}\n{GetBattleCommandPromptMessage()}";
    }

    private string GetBattleSpellPromptMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "Choose a spell."
            : "どのじゅもんをつかう？";
    }

    private string GetBattleItemPromptMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "Choose an item."
            : "なにを つかう？";
    }

    private string GetBattleEquipmentPromptMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "Choose gear."
            : "なにを そうびする？";
    }

    private string GetBattleSelectionPromptMessage(BattleFlowState selectionState)
    {
        return selectionState switch
        {
            BattleFlowState.SpellSelection => GetBattleSpellPromptMessage(),
            BattleFlowState.ItemSelection => GetBattleItemPromptMessage(),
            BattleFlowState.EquipmentSelection => GetBattleEquipmentPromptMessage(),
            _ => GetBattleCommandPromptMessage()
        };
    }

    private string GetBattleSelectionMessage(BattleFlowState selectionState)
    {
        var prompt = GetBattleSelectionPromptMessage(selectionState);
        if (selectionState != BattleFlowState.SpellSelection)
        {
            return prompt;
        }

        var entries = GetBattleSpellEntries();
        if (entries.Count == 0)
        {
            return prompt;
        }

        var selectedIndex = Math.Clamp(battleListCursor, 0, entries.Count - 1);
        var selectedSpell = entries[selectedIndex].Spell;
        if (selectedSpell is null)
        {
            return prompt;
        }

        var description = GameContent.GetSpellDescription(selectedSpell, selectedLanguage);
        return string.IsNullOrWhiteSpace(description)
            ? prompt
            : $"{prompt}\n{description}";
    }

    private string GetBattleNoSpellsMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "You do not know any spells."
            : "じゅもんを おぼえていない。";
    }

    private string GetBattleNoItemsMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "You have no usable items."
            : "つかえる どうぐがない。";
    }

    private string GetBattleNoEquipmentMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "No gear to switch."
            : "つけかえられる そうびがない。";
    }

    private string GetBattleEmptySelectionMessage(BattleFlowState selectionState)
    {
        return selectionState switch
        {
            BattleFlowState.SpellSelection => GetBattleNoSpellsMessage(),
            BattleFlowState.ItemSelection => GetBattleNoItemsMessage(),
            BattleFlowState.EquipmentSelection => GetBattleNoEquipmentMessage(),
            _ => GetBattleCommandPromptMessage()
        };
    }

    private string GetBattleCommandHelpMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "D-PAD/LS/WASD: CHOOSE\nA/Y/ENTER/Z: OK  B/ESC: RUN"
            : "十字/LS/WASD: せんたく\nA/Y/ENTER/Z: けってい  B/ESC: にげる";
    }

    private string GetBattleSubmenuHelpMessage()
    {
        return selectedLanguage == UiLanguage.English
            ? "D-PAD/LS/WASD: CHOOSE\nA/Y/ENTER/Z: OK  B/X/ESC: BACK"
            : "十字/LS/WASD: せんたく\nA/Y/ENTER/Z: けってい  B/X/ESC: もどる";
    }

    private string GetBattleSelectionTitle()
    {
        return battleFlowState switch
        {
            BattleFlowState.SpellSelection => selectedLanguage == UiLanguage.English ? "SPELL" : "じゅもん",
            BattleFlowState.ItemSelection => selectedLanguage == UiLanguage.English ? "ITEM" : "どうぐ",
            BattleFlowState.EquipmentSelection => selectedLanguage == UiLanguage.English ? "EQUIP" : "そうび",
            _ => selectedLanguage == UiLanguage.English ? "COMMAND" : "こうどう"
        };
    }

    private int GetBattleCommandRowCount()
    {
        return GameContent.BattleCommandGrid.GetLength(0);
    }

    private int GetBattleCommandColumnCount()
    {
        return GameContent.BattleCommandGrid.GetLength(1);
    }

    private string GetBattleCommandLabel(int row, int column)
    {
        return GameContent.GetBattleCommandLabel(selectedLanguage, row, column);
    }

    private IReadOnlyList<BattleSelectionEntry> GetBattleSpellEntries()
    {
        return battleService.GetKnownSpells(player)
            .Select(spell => new BattleSelectionEntry(
                GameContent.GetSpellName(spell, selectedLanguage),
                GetBattleSpellDetail(spell),
                $"MP {spell.MpCost}",
                Spell: spell))
            .ToArray();
    }

    private IReadOnlyList<BattleSelectionEntry> GetBattleItemEntries()
    {
        return GameContent.ConsumableCatalog
            .Where(item => player.GetItemCount(item.Id) > 0)
            .Select(item => new BattleSelectionEntry(
                GameContent.GetConsumableName(item, selectedLanguage),
                GetBattleConsumableDetail(item),
                GetBattleCountBadge(player.GetItemCount(item.Id)),
                Consumable: item))
            .ToArray();
    }

    private IReadOnlyList<BattleSelectionEntry> GetBattleEquipmentEntries()
    {
        var weaponEntries = GameContent.WeaponCatalog
            .Where(item => player.GetItemCount(item.Id) > 0 &&
                !string.Equals(player.EquippedWeaponId, item.Id, StringComparison.Ordinal))
            .Select(item => new BattleSelectionEntry(
                GameContent.GetWeaponName(item, selectedLanguage),
                GetBattleEquipmentDetail(item),
                GetBattleCountBadge(player.GetItemCount(item.Id)),
                Equipment: item));

        var armorEntries = GameContent.ArmorCatalog
            .Where(item => player.GetItemCount(item.Id) > 0 &&
                !string.Equals(player.GetEquippedItemId(item.Slot), item.Id, StringComparison.Ordinal))
            .OrderBy(item => item.Slot)
            .ThenBy(item => item.Price)
            .Select(item => new BattleSelectionEntry(
                GameContent.GetArmorName(item, selectedLanguage),
                GetBattleEquipmentDetail(item),
                GetBattleCountBadge(player.GetItemCount(item.Id)),
                Equipment: item));

        return weaponEntries.Concat(armorEntries).ToArray();
    }

    private IReadOnlyList<BattleSelectionEntry> GetActiveBattleSelectionEntries()
    {
        return battleFlowState switch
        {
            BattleFlowState.SpellSelection => GetBattleSpellEntries(),
            BattleFlowState.ItemSelection => GetBattleItemEntries(),
            BattleFlowState.EquipmentSelection => GetBattleEquipmentEntries(),
            _ => []
        };
    }

    private string GetBattleSelectionCounterText()
    {
        var entries = GetActiveBattleSelectionEntries();
        if (entries.Count == 0)
        {
            return "0/0";
        }

        return $"{battleListCursor + 1}/{entries.Count}";
    }

    private string GetBattleSpellDetail(SpellDefinition spell)
    {
        return spell.EffectType switch
        {
            SpellEffectType.DamageEnemy => selectedLanguage == UiLanguage.English ? $"DMG {spell.Power}" : $"与D {spell.Power}",
            SpellEffectType.HealPlayer => $"HP+{spell.Power}",
            SpellEffectType.PoisonEnemy => selectedLanguage == UiLanguage.English ? "POISON" : "どく",
            SpellEffectType.SleepEnemy => selectedLanguage == UiLanguage.English ? "SLEEP" : "ねむり",
            SpellEffectType.CurePlayerStatus => selectedLanguage == UiLanguage.English ? "CURE" : "なおす",
            _ => GameContent.GetSpellDescription(spell, selectedLanguage)
        };
    }

    private string GetBattleConsumableDetail(ConsumableDefinition item)
    {
        return item.EffectType switch
        {
            ConsumableEffectType.HealHp => $"HP+{item.Amount}",
            ConsumableEffectType.HealMp => $"MP+{item.Amount}",
            ConsumableEffectType.DamageEnemy => selectedLanguage == UiLanguage.English ? $"DMG {item.Amount}" : $"与D {item.Amount}",
            _ => GameContent.GetConsumableDescription(item, selectedLanguage)
        };
    }

    private string GetBattleEquipmentDetail(IEquipmentDefinition equipment)
    {
        return equipment.Slot == EquipmentSlot.Weapon
            ? $"ATK {equipment.AttackBonus}{FormatSignedStat(equipment.AttackBonus - (GetEquippedWeapon()?.AttackBonus ?? 0))}"
            : $"{GetEquipmentSlotLabel(equipment.Slot)} DEF {equipment.DefenseBonus}{FormatSignedStat(equipment.DefenseBonus - (GetEquippedArmor(equipment.Slot)?.DefenseBonus ?? 0))}";
    }

    private string GetBattleCountBadge(int count)
    {
        return selectedLanguage == UiLanguage.English
            ? $"x{count}"
            : $"×{count}";
    }
}
