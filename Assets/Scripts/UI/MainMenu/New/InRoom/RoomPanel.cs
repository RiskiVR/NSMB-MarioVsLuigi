﻿using NSMB.Translation;
using Quantum;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus {
    public class RoomPanel : InRoomSubmenuPanel {

        //---Properties
        public override GameObject DefaultSelectedObject => rules[0].gameObject;
        public override bool IsInSubmenu => rules.Any(r => r.Editing);

        //---Serialized Variables
        [SerializeField] private Image stagePreviewImage;
        [SerializeField] private TMP_Text stageNameText, rulesText;
        [SerializeField] private StagePreviewManager stagePreviewManager;
        [SerializeField] private MainMenuChat chat;

        //---Private Variables
        private readonly List<ChangeableRule> rules = new();
        private VersusStageData currentStage;

        public override void Initialize() {
            base.Initialize();
            GetComponentsInChildren(true, rules);
            foreach (var rule in rules) {
                rule.Initialize();
            }
            chat.Initialize();
        }

        public unsafe void Start() {
            if (NetworkHandler.Runner && NetworkHandler.Runner.Game != null) {
                Frame f = NetworkHandler.Runner.Game.Frames.Predicted;
                UpdateRules(f.Global->Rules);
                ChangeStage(f.FindAsset<VersusStageData>(f.FindAsset(f.Global->Rules.Stage).UserAsset));
            }

            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
            QuantumEvent.Subscribe<EventRulesChanged>(this, OnRulesChanged);
            TranslationManager.OnLanguageChanged += OnLanguageChanged;
        }

        public void OnDestroy() {
            TranslationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        public override bool TryGoBack(out bool playSound) {
            if (rules.Any(r => r.Editing)) {
                foreach (var rule in rules) {
                    rule.Editing = false;
                }
                playSound = true;
                return false;
            }
            return base.TryGoBack(out playSound);
        }

        private void UpdateRules(in GameRules rules) {
            TranslationManager tm = GlobalController.Instance.translationManager;
            StringBuilder builder = new();
            builder.Append("<sprite name=room_stars> ").AppendLine(rules.StarsToWin.ToString());
            builder.Append("<sprite name=room_coins> ").AppendLine(rules.CoinsForPowerup.ToString());
            builder.Append("<sprite name=room_lives> ").AppendLine(rules.Lives > 0 ? rules.Lives.ToString() : "∞");
            builder.Append("<sprite name=room_timer> ").AppendLine(rules.TimerSeconds > 0 ? Utils.Utils.SecondsToMinuteSeconds(rules.TimerSeconds) : tm.GetTranslation("ui.generic.off"));
            builder.Append("<sprite name=room_powerups>").AppendLine(tm.GetTranslation(rules.CustomPowerupsEnabled ? "ui.generic.on" : "ui.generic.off"));
            builder.Append("<sprite name=room_teams>").Append(tm.GetTranslation(rules.TeamsEnabled ? "ui.generic.on" : "ui.generic.off"));

            rulesText.text = builder.ToString();
        }

        private void ChangeStage(VersusStageData newStage) {
            stageNameText.text = GlobalController.Instance.translationManager.GetTranslation(newStage.TranslationKey);
            stagePreviewImage.sprite = newStage.Icon;
            currentStage = newStage;
        }

        private unsafe void OnGameStarted(CallbackGameStarted e) {
            Frame f = e.Game.Frames.Predicted;
            UpdateRules(f.Global->Rules);
            ChangeStage(f.FindAsset<VersusStageData>(f.FindAsset(f.Global->Rules.Stage).UserAsset));
        }

        private unsafe void OnRulesChanged(EventRulesChanged e) {
            Frame f = e.Frame;
            ref GameRules rules = ref f.Global->Rules;

            if (e.LevelChanged) {
                ChangeStage(f.FindAsset<VersusStageData>(f.FindAsset(rules.Stage).UserAsset));
            }

            UpdateRules(rules);
        }

        private void OnLanguageChanged(TranslationManager tm) {
            if (!currentStage) {
                return;
            }

            stageNameText.text = tm.GetTranslation(currentStage.TranslationKey);
        }
    }
}