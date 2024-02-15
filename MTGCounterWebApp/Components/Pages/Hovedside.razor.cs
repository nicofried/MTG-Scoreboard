using System;
using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using SE_MTG;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MTGCounter.Components.Pages
{
    public partial class Hovedside : IDisposable
    {
        private System.Threading.Timer? _timer;
        private const int PollingInterval = 50; // Adjusted to 1 second

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        // Player HP properties
        public int Player1HP { get; set; }
        public int Player2HP { get; set; }
        public int Player3HP { get; set; }
        public int Player4HP { get; set; }
        public int Player5HP { get; set; }
        public int Player6HP { get; set; }
        public int Player7HP { get; set; }
        public int Player8HP { get; set; }
        public int Amountofplayers { get; set; } = Form1.Instance.AmountofPlayers;
        public string Playername1 { get; set; } = Form1.Instance.Player1Name;
        public string Playername2 { get; set; } = Form1.Instance.Player2Name;
        public string Playername3 { get; set; } = Form1.Instance.Player3Name;
        public string Playername4 { get; set; } = Form1.Instance.Player4Name;

        public string Playername5 { get; set; } = Form1.Instance.Player5Name;
        public string Playername6 { get; set; } = Form1.Instance.Player6Name;
        public string Playername7 { get; set; } = Form1.Instance.Player7Name;
        public string Playername8 { get; set; } = Form1.Instance.Player8Name;
        protected override void OnInitialized()
        {
            _timer?.Dispose(); // Ensure no timer is running before creating a new one
            _timer = new System.Threading.Timer(Callback, null, 0, PollingInterval);
        }

        private void Callback(object? state)
        {
            InvokeAsync(() =>
            {
                Playername1 = Form1.Instance.Player1Name;
                Playername2 = Form1.Instance.Player2Name;
                Playername3 = Form1.Instance.Player3Name;
                Playername4 = Form1.Instance.Player4Name;
                Playername5 = Form1.Instance.Player5Name;
                Playername6 = Form1.Instance.Player6Name;
                Playername7 = Form1.Instance.Player7Name;
                Playername8 = Form1.Instance.Player8Name;
                // Example of how you might synchronize player HPs from an external source
                SyncPlayerHPFromExternalSource();
                StateHasChanged(); // Request the UI to refresh
            });
        }

        private void SyncPlayerHPFromExternalSource()
        {
            // Synchronize each player's HP from the external source
            Player1HP = Form1.Instance.returnPlayer1HP;
            Player2HP = Form1.Instance.returnPlayer2HP;
            Player3HP = Form1.Instance.returnPlayer3HP;
            Player4HP = Form1.Instance.returnPlayer4HP;
            Player5HP = Form1.Instance.returnPlayer5HP;
            Player6HP = Form1.Instance.returnPlayer6HP;
            Player7HP = Form1.Instance.returnPlayer7HP;
            Player8HP = Form1.Instance.returnPlayer8HP;
            Amountofplayers = Form1.Instance.AmountofPlayers;
        }

        private void UpdatePlayerHP(int playerNumber, int newHp)
        {
            switch (playerNumber)
            {
                case 1:
                    Player1HP = newHp;
                    Form1.Instance.returnPlayer1HP = newHp;
                    break;
                case 2:
                    Player2HP = newHp;
                    Form1.Instance.returnPlayer2HP = newHp;
                    break;
                case 3:
                    Player3HP = newHp;
                    Form1.Instance.returnPlayer3HP = newHp;
                    break;
                case 4:
                    Player4HP = newHp;
                    Form1.Instance.returnPlayer4HP = newHp;
                    break;
                case 5:
                    Player5HP = newHp;
                    Form1.Instance.returnPlayer5HP = newHp;
                    break;
                case 6:
                    Player6HP = newHp;
                    Form1.Instance.returnPlayer6HP = newHp;
                    break;
                case 7:
                    Player7HP = newHp;
                    Form1.Instance.returnPlayer7HP = newHp;
                    break;
                case 8:
                    Player8HP = newHp;
                    Form1.Instance.returnPlayer8HP = newHp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerNumber), $"Player number {playerNumber} is out of range.");
            }
        }

        // In Hovedside.razor.cs
        public int GetPlayerHP(int playerNumber)
        {
            return playerNumber switch
            {
                1 => Player1HP,
                2 => Player2HP,
                3 => Player3HP,
                4 => Player4HP,
                5 => Player5HP,
                6 => Player6HP,
                7 => Player7HP,
                8 => Player8HP,
                _ => 0 // Provide a default case for any number not between 1 and 8
            };
        }

        public string GetPlayerName(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1: return Playername1;
                case 2: return Playername2;
                case 3: return Playername3;
                case 4: return Playername4;
                case 5: return Playername5;
                case 6: return Playername6;
                case 7: return Playername7;
                case 8: return Playername8;
                default: return "Unknown Player";
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
