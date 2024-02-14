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
    public partial class Player6
    {
        public string Playername6 { get; set; } = Form1.Instance.Player6Name;
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

        protected async Task Button0ClickPlayer6(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            Form1.Instance.BeginInvoke(() => Form1.Instance.Treachery6Unveil_Click(this, args));
        }


        public string TreacheryImagePath { get; set; } = Form1.Instance.Treachery6ImagePath;
        public string TreacheryRuleText { get; set; } = Form1.Instance.TreacheryRule6Path;

        protected override async Task OnInitializedAsync()
        {
            TreacheryImagePath = "images/Treachery/" + Form1.Instance.Treachery6ImagePath;
            TreacheryRuleText = Form1.Instance.TreacheryRule6Path;

            //Console.WriteLine($"Full Image Path: {TreacheryImagePath}");
            //Console.WriteLine($"Rule Text: {TreacheryRuleText}");
        }
        public string PasswordP6 { get; set; } = Form1.Instance.ReturnP6Password;

        private void HandleValidSubmit()
        {
            // Check if the entered password is correct
            if (inputPassword == PasswordP6)
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

        private bool MustChangePassword => isAuthenticated && PasswordP6 == "1234" && !passwordHasBeenSet;


        private void SetNewPassword()
        {
            if (!string.IsNullOrWhiteSpace(newPassword) && newPassword != "1234")
            {
                PasswordP6 = newPassword; // Update the local password
                Form1.Instance.ReturnP6Password = newPassword; // Update the password in Form1.Instance
                newPassword = string.Empty; // Optionally clear the input field
                passwordHasBeenSet = true; // Indicate that the password has been set
                JSRuntime.InvokeVoidAsync("alert", "Password updated successfully!");
                StateHasChanged(); // Refresh UI to reflect the change
            }
            else
            {
                JSRuntime.InvokeVoidAsync("alert", "New password cannot be empty or the default password!");
            }
        }
    }
}
