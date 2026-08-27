using System.Text;
using MoonshineSim.Core;
using MoonshineSim.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MoonshineSim.UI
{
    /// <summary>
    /// The clipboard — the game's only screen UI. Toggle with Tab. Shows the
    /// batch checklist, live batch data, and the back-door buyers' standing
    /// preferences (the one place demand is legible). Read-only; it ticks
    /// itself from <see cref="BatchController"/> and <see cref="StillRunController"/>.
    /// </summary>
    public class ClipboardController : MonoBehaviour
    {
        [SerializeField] private BatchController batch;
        [SerializeField] private GameState gameState;
        [SerializeField] private StillRunController stillRun;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text bodyText;
        [SerializeField] private Key toggleKey = Key.Tab;
        [SerializeField] private bool startVisible;

        private readonly ClipboardModel _model = new();
        private float _targetProof;
        private float _shownProof;

        private static readonly string[] StepIds =
            { "plan", "mash", "ferment", "distill", "proof", "sell" };

        private void Awake()
        {
            if (batch == null) batch = BatchController.Instance;
            if (gameState == null) gameState = GameState.Instance;
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();

            SeedChecklist();
            _model.Changed += Redraw;

            if (batch != null)
            {
                batch.OnStageChanged += HandleStage;
                batch.OnProofChanged += HandleProof;
                batch.OnSold += HandleSold;
            }
            if (stillRun != null)
            {
                stillRun.OnCutMade += HandleCut;
            }
            if (gameState != null)
            {
                gameState.OnUpgradePurchased += HandleUpgrade;
            }

            RefreshBuyers();
            RefreshWallet();
            if (panelRoot != null) panelRoot.SetActive(startVisible);
            Redraw();
        }

        private void OnDestroy()
        {
            _model.Changed -= Redraw;
            if (batch != null)
            {
                batch.OnStageChanged -= HandleStage;
                batch.OnProofChanged -= HandleProof;
                batch.OnSold -= HandleSold;
            }
            if (stillRun != null) stillRun.OnCutMade -= HandleCut;
            if (gameState != null) gameState.OnUpgradePurchased -= HandleUpgrade;
        }

        private void HandleUpgrade(Upgrade u) => RefreshWallet();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && panelRoot != null && kb[toggleKey].wasPressedThisFrame)
            {
                panelRoot.SetActive(!panelRoot.activeSelf);
            }

            // Smooth the proof readout so the tier-1 gauge sway doesn't flicker.
            if (!Mathf.Approximately(_shownProof, _targetProof))
            {
                _shownProof = Mathf.Lerp(_shownProof, _targetProof, 1f - Mathf.Exp(-4f * Time.deltaTime));
                if (Mathf.Abs(_shownProof - _targetProof) < 0.25f) _shownProof = _targetProof;
                _model.SetData("Proof", $"{_shownProof:0}");
            }
        }

        private void SeedChecklist()
        {
            _model.AddItem("plan", "Pick a grain");
            _model.AddItem("mash", "Cook the mash", available: false);
            _model.AddItem("ferment", "Ferment the wash", available: false);
            _model.AddItem("distill", "Run the still", available: false);
            _model.AddItem("proof", "Proof it down", available: false);
            _model.AddItem("sell", "Sell to a buyer", available: false);
        }

        private void HandleStage(BatchStage s)
        {
            _model.SetData("Stage", Nice(s));
            if (batch != null) _model.SetData("Grain", batch.Batch.style.ToString());

            if (s == BatchStage.GrainChosen)
            {
                _targetProof = _shownProof = 0f;
                _model.SetData("Proof", "0");
            }

            if (s == BatchStage.None || s == BatchStage.Sold)
            {
                foreach (var id in StepIds) _model.SetDone(id, false);
                if (s == BatchStage.Sold) _model.SetDone("sell");
                return;
            }

            _model.SetDone("plan", s >= BatchStage.GrainChosen);
            _model.SetAvailable("mash", s >= BatchStage.GrainChosen);
            _model.SetDone("mash", s >= BatchStage.Mashed);
            _model.SetAvailable("ferment", s >= BatchStage.Mashed);
            _model.SetDone("ferment", s >= BatchStage.WashReady);
            _model.SetAvailable("distill", s >= BatchStage.WashReady);
            _model.SetDone("distill", s >= BatchStage.Distilled);
            _model.SetAvailable("proof", s >= BatchStage.Distilled);
            _model.SetDone("proof", s >= BatchStage.Proofed);
            _model.SetAvailable("sell", s >= BatchStage.Distilled);
        }

        private void HandleProof(float proof) => _targetProof = proof;

        private void HandleCut(string label, float quality)
        {
            if (label == "hearts_started") _model.SetData("Hearts cut", $"{quality * 100f:0}%");
        }

        private void HandleSold(Buyer buyer, Appraisal appraisal)
        {
            _model.SetData("Last sale", $"{buyer.name} paid ${appraisal.price} ({appraisal.matchScore * 100f:0}% match)");
            RefreshBuyers();
            RefreshWallet();
        }

        private void RefreshWallet()
        {
            if (gameState == null) return;
            _model.SetData("Cash", $"${gameState.Cash}");
            _model.SetData("Rapport", gameState.Rapport.ToString());
            _model.SetData("Next upgrade", gameState.IsLicensed
                ? "LICENSED"
                : $"{UpgradeTrack.Steps[gameState.UpgradeLevel].name}  (${UpgradeTrack.Steps[gameState.UpgradeLevel].cost})");
        }

        private void RefreshBuyers()
        {
            if (batch == null || batch.Buyers == null || batch.Buyers.Length == 0) return;
            var sb = new StringBuilder();
            foreach (var b in batch.Buyers)
            {
                string lean = b.smoothLean > 0.2f ? "smooth" : b.smoothLean < -0.2f ? "bold" : "either";
                sb.Append("• ").Append(b.name).Append(" — ")
                  .Append(Spirits.DrinkName(b.preferredStyle)).Append(' ')
                  .Append($"{b.proofMin:0}-{b.proofMax:0}pf, ").Append(lean)
                  .Append(", rep ").Append(b.rapport).Append('\n');
            }
            _model.SetNote("BUYERS", sb.ToString());
        }

        private static string Nice(BatchStage s) => s switch
        {
            BatchStage.None => "idle",
            BatchStage.GrainChosen => "grain picked",
            BatchStage.Mashing => "mashing",
            BatchStage.Mashed => "mash ready",
            BatchStage.Fermenting => "fermenting",
            BatchStage.WashReady => "wash ready",
            BatchStage.Distilling => "distilling",
            BatchStage.Distilled => "needs proofing",
            BatchStage.Proofed => "ready to sell",
            BatchStage.Sold => "sold",
            _ => s.ToString()
        };

        private void Redraw()
        {
            if (bodyText != null) bodyText.text = _model.Render();
        }
    }
}
