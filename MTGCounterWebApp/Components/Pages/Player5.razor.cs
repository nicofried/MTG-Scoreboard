using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using SE_MTG;

namespace MTGCounter.Components.Pages
{
    public partial class Player5 : IDisposable
    {
        private System.Threading.Timer? _timer;
        private const int PollingInterval = 1000; // Adjusted to 1 second
        public string Playername5 { get; set; } = Form1.Instance.Player5Name;


        private string inputPassword;
        private bool isAuthenticated = false;

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

        protected async Task Button0ClickPlayer5(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            Form1.Instance.BeginInvoke(() => Form1.Instance.Treachery5Unveil_Click(this, args));
        }


        public string TreacheryImagePath { get; set; } = Form1.Instance.Treachery5ImagePath;
        public string TreacheryRuleText { get; set; } = Form1.Instance.TreacheryRule5Path;

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            TreacheryImagePath = "images/Treachery/" + Form1.Instance.Treachery5ImagePath;
            TreacheryRuleText = Form1.Instance.TreacheryRule5Path;
            _timer?.Dispose(); // Ensure no timer is running before creating a new one
            _timer = new System.Threading.Timer(Callback, null, 0, PollingInterval);
            //Console.WriteLine($"Amount of players: {Amountofplayers}");

            //Console.WriteLine($"Full Image Path: {TreacheryImagePath}");
            //Console.WriteLine($"Rule Text: {TreacheryRuleText}");
        }
        public string PasswordP5 { get; set; } = Form1.Instance.ReturnP5Password;

        private void HandleValidSubmit()
        {
            // Check if the entered password is correct
            if (inputPassword == PasswordP5)
            {
                isAuthenticated = true;
            }
            else
            {
                JSRuntime.InvokeVoidAsync("alert", "Incorrect password!");
                inputPassword = string.Empty; // Reset input
            }
        }

        private string newPassword; // Add this line to store the new password
        private bool passwordHasBeenSet = false; // Add this flag
        private bool showSettingsWindow = false;

        private bool MustChangePassword => PasswordP5 == "1234" && !passwordHasBeenSet;

        private void Submit()
        {
            SetPlayerName();
            if (!string.IsNullOrEmpty(newPassword))
            {
                SetNewPassword();
            }

            showSettingsWindow = false;
        }

        private void SetPlayerName()
        {
            Form1.Instance.Player5Name = Playername5; // Update the password in Form1.
            StateHasChanged(); // Refresh UI to reflect the change
        }
        // Method to handle changing the 


        private void Settings()
        {
            showSettingsWindow = !showSettingsWindow;
            StateHasChanged();

        }
        private void SetNewPassword()
        {
            if (!string.IsNullOrWhiteSpace(newPassword) && newPassword != "1234")
            {
                PasswordP5 = newPassword; // Update the local password
                Form1.Instance.ReturnP5Password = newPassword; // Update the password in Form1.Instance
                newPassword = string.Empty; // Optionally clear the input field
                passwordHasBeenSet = true; // Indicate that the password has been set
                isAuthenticated = true;
                //JSRuntime.InvokeVoidAsync("alert", "Password updated successfully!");
                StateHasChanged(); // Refresh UI to reflect the change
            }
            else
            {
                JSRuntime.InvokeVoidAsync("alert", "New password cannot be empty or the default password!");
            }
        }
        private void Callback(object? state)
        {
            InvokeAsync(() =>
            {
                // Fetch updated data from your forms
                Playername5 = Form1.Instance.Player5Name;
                TreacheryImagePath = "images/Treachery/" + Form1.Instance.Treachery5ImagePath;
                TreacheryRuleText = Form1.Instance.TreacheryRule5Path;
                PasswordP5 = Form1.Instance.ReturnP5Password;
                if (PasswordP5 == "1234")
                {
                    showSettingsWindow = true;
                }
                StateHasChanged(); // Request the UI to refresh
            });
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
