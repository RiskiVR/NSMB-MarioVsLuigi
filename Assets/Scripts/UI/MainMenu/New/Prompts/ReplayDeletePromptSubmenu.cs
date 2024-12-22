using System.IO;
using TMPro;
using UnityEngine;
using static ReplayListManager;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class ReplayDeletePromptSubmenu : PromptSubmenu {

        //---Serialized Variables
        [SerializeField] private ReplayListManager manager;
        [SerializeField] private TMP_Text text;

        //---Private Variables
        private Replay target;
        private bool success;

        public void Open(Replay replay) {
            target = replay;
            Canvas.OpenMenu(this);
        }

        public override void Show(bool first) {
            base.Show(first);
            success = false;
            text.text = GlobalController.Instance.translationManager.GetTranslationWithReplacements("ui.extras.replays.delete.text", 
                "replayname", target.ReplayFile.GetDisplayName());
        }

        public override bool TryGoBack(out bool playSound) {
            if (success) {
                Canvas.PlayConfirmSound();
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        public void ClickConfirm() {
            File.Delete(target.FilePath);
            manager.RemoveReplay(target);
            target = null;
            success = true;
            Canvas.GoBack();
        }
    }
}