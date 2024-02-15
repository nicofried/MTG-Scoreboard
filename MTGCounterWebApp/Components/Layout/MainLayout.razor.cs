using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using SE_MTG;
using System.Threading;

namespace MTGCounter.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        private System.Threading.Timer? _timer;
        private const int PollingInterval = 1000; // Adjusted to 1 second

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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _timer?.Dispose(); // Ensure no timer is running before creating a new one
            _timer = new System.Threading.Timer(Callback, null, 0, PollingInterval);
            sidebarExpanded = false;
            //Console.WriteLine($"Amount of players: {Amountofplayers}");
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            // Logic to handle updates to Amountofplayers, if it changes dynamically
        }


        private bool sidebarExpanded = true;

        public int Amountofplayers { get; set; } = Form1.Instance.AmountofPlayers;
        public string Playername1 { get; set; } = Form1.Instance.Player1Name;
        public string Playername2 { get; set; } = Form1.Instance.Player2Name;
        public string Playername3 { get; set; } = Form1.Instance.Player3Name;
        public string Playername4 { get; set; } = Form1.Instance.Player4Name;

        public string Playername5 { get; set; } = Form1.Instance.Player5Name;
        public string Playername6 { get; set; } = Form1.Instance.Player6Name;
        public string Playername7 { get; set; } = Form1.Instance.Player7Name;
        public string Playername8 { get; set; } = Form1.Instance.Player8Name;

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


        private void Callback(object? state)
        {
            InvokeAsync(() =>
            {
                // Fetch updated data from your forms
                Amountofplayers = Form1.Instance.AmountofPlayers;
                Playername1 = Form1.Instance.Player1Name;
                Playername2 = Form1.Instance.Player2Name;
                Playername3 = Form1.Instance.Player3Name;
                Playername4 = Form1.Instance.Player4Name;
                Playername5 = Form1.Instance.Player5Name;
                Playername6 = Form1.Instance.Player6Name;
                Playername7 = Form1.Instance.Player7Name;
                Playername8 = Form1.Instance.Player8Name;

                StateHasChanged(); // Request the UI to refresh
            });
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
        }
    }
}
