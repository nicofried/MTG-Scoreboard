using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using QRCoder;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Net;
using static QRCoder.PayloadGenerator;


namespace SE_MTG
{
    public partial class Form1 : Form
    {
        private Form imagePopupForm = new Form();
        public int selectedPlayers = 8;
        private double[] playerDamage = new double[TotalPlayers];
        private const int TotalPlayers = 8;
        private CustomProgressBar[,] playerCommanderDmgBars = new CustomProgressBar[TotalPlayers, TotalPlayers];
        private ProgressBar[] playerHPBars = new ProgressBar[TotalPlayers]; // Assuming you have HP bars similar to damage bars
        private CustomProgressBar[] playerPoisonDmgBars = new CustomProgressBar[TotalPlayers];
        private const int MaxPlayers = 8;
        private int scrollValue = 0;
        private ToolTip scrollToolTip = new ToolTip();
        private Timer scrollTimer = new Timer();
        private readonly Color[] playerColors = new Color[TotalPlayers]


        {
            Color.LightBlue, // Player 1
            Color.Red, // Player 2
            Color.Turquoise, // Player 3
            Color.Green, // Player 4
            Color.Orange, // Player 5
            Color.Purple, // Player 6
            Color.Brown, // Player 7
            Color.Magenta // Player 8
        };
        private Form rulesForm = null; // Declare a class-level variable to track the rules form
        private WebSocketServer webSocketServer;
        private static Form1 instance;
        private string wifi = "Schneider Tosi Wifi";
        private string wifipass = "Syst3m22_";


        public static Form1 Instance
        {
            get
            {
                return instance;
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string GetExternalIPAddress()
        {
            using (WebClient webClient = new WebClient())
            {
                string externalIP = webClient.DownloadString("http://icanhazip.com").Trim();
                return externalIP;
            }
        }



        public Bitmap GenerateQrCode(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }

        private void DisplayQRCode()
        {
            // Assuming you want to use the machine's IP and port 5001, you might have to resolve the IP dynamically.
            // For simplicity, we'll use a placeholder. Replace "YourCurrentIP" with your actual IP or hostname.
            string localIP = GetLocalIPAddress();
            string url = $"https://{localIP}:5001";
            Bitmap qrCodeImage = GenerateQrCode(url);
            QRCode.Image = qrCodeImage; // Assuming QRCode is your PictureBox's name
            QRCode.SizeMode = PictureBoxSizeMode.StretchImage;

        }




        //ON STARTUP FUNCTIONS
        public Form1()
        {
            instance = this;
            InitializeComponent();

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the WebSocket server when the form is closing
            //StopWebSocketServer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize your UI elements here
            InitializeImagePopupForm();
            InitializeCommandComboBox1();
            DisplayQRCode(); // Display the QR code

            //StartWebSocketServer();

            for (int i = 1; i <= 8; i++)
            {
                CommanderSelector.Items.Add($"CommanderImage{i}");
            }
            // After populating the CommanderSelector ComboBox
            CommanderSelector.SelectedIndex = CommanderSelector.Items.IndexOf("CommanderImage1");


            InitializeNewGame();
            ResetPasswords();


            GenerateWifiQRCode(wifi, wifipass);

            // Initialize player text boxes and progress bars
            InitializePlayerTextBoxes();

            for (int player = 1; player <= TotalPlayers; player++)
            {
                for (int opponent = 1; opponent <= TotalPlayers; opponent++)
                {
                    if (player != opponent) // A player does not take commander damage from themselves
                    {
                        InitializeCommanderDmgBar(player, opponent);
                    }
                }
            }

            // Debugging code to check if the progress bars are correctly assigned
            for (int i = 0; i < TotalPlayers; i++)
            {
                for (int j = 0; j < TotalPlayers; j++)
                {
                    if (playerCommanderDmgBars[i, j] != null)
                    {
                        //Console.WriteLine($"Bar for player {i + 1} vs opponent {j + 1} is set.");
                    }
                    else
                    {
                        //Console.WriteLine($"Bar for player {i + 1} vs opponent {j + 1} is NOT set.");
                    }
                }
            }

            for (int player = 1; player <= TotalPlayers; player++)
            {
                InitializePoisonDmgBar(player);
            }

            TreacheryImage1 = "Treachery1.jpg";
            TreacheryImage2 = "Treachery1.jpg";
            TreacheryImage3 = "Treachery1.jpg";
            TreacheryImage4 = "Treachery1.jpg";
            TreacheryImage5 = "Treachery1.jpg";
            TreacheryImage6 = "Treachery1.jpg";
            TreacheryImage7 = "Treachery1.jpg";
            TreacheryImage8 = "Treachery1.jpg";

        }


        //Initialize Functions
        private void InitializePlayerTextBoxes()
        {
            for (int player = 1; player <= TotalPlayers; player++)
            {
                TextBox playerTextBox = this.Controls["Player" + player] as TextBox;
                if (playerTextBox != null)
                {
                    playerTextBox.ForeColor = GetPlayerColor(player);
                }
            }
        }
        private void InitializePoisonDmgBar(int player)
        {
            string progressBarName = $"Player{player}PoisonDMG";
            CustomProgressBar poisonBar = this.Controls.Find(progressBarName, true).FirstOrDefault() as CustomProgressBar;

            if (poisonBar != null)
            {
                poisonBar.MouseClick += PoisonDmgBar_MouseClick;
                poisonBar.Maximum = 10;  // Maximum poison damage is 10
                poisonBar.StartColor = Color.Green;  // Start color for the gradient
                poisonBar.EndColor = Color.Purple;  // End color for the gradient
                poisonBar.TextStartColor = Color.White;  // Color for the text

                // Set the bar type to Poison
                poisonBar.BarType = CustomProgressBar.ProgressBarType.Poison;

                playerPoisonDmgBars[player - 1] = poisonBar;
                //Console.WriteLine($"Initialized {progressBarName} and stored in array.");
            }
            else
            {
                //Console.WriteLine($"Poison damage bar '{progressBarName}' not found. Initialization skipped.");
            }
        }
        private void InitializeCommanderDmgBar(int player, int opponent)
        {
            string progressBarName = $"P{opponent}CommanderDMGP{player}";
            CustomProgressBar dmgBar = this.Controls.Find(progressBarName, true).FirstOrDefault() as CustomProgressBar;

            if (dmgBar != null)
            {
                dmgBar.MouseClick += CommanderDmgBar_MouseClick;
                dmgBar.Maximum = 21;
                dmgBar.StartColor = GetPlayerColor(opponent);
                dmgBar.EndColor = Color.DarkRed;
                dmgBar.TextStartColor = GetPlayerColor(opponent);

                // Set the bar type to Commander
                dmgBar.BarType = CustomProgressBar.ProgressBarType.Commander;

                // Set the PlayerName to the opponent's number (the one dealing the damage)
                dmgBar.PlayerName = $"Player {opponent}";

                playerCommanderDmgBars[player - 1, opponent - 1] = dmgBar;
                //Console.WriteLine($"Initialized {progressBarName} and stored in array.");
            }
            else
            {
                //Console.WriteLine($"Progress bar '{progressBarName}' not found. Initialization skipped.");
            }
        }
        private void InitializeCommandComboBox1()
        {
            string cardsFolderPath = Path.Combine(Application.StartupPath, "Cards");

            if (Directory.Exists(cardsFolderPath))
            {
                // Get all files (cards) in the "Cards" folder
                string[] cardFiles = Directory.GetFiles(cardsFolderPath, "*.jpg");

                // Extract just the card names (without the file extension)
                List<string> cardNames = cardFiles.Select(filePath => Path.GetFileNameWithoutExtension(filePath)).ToList();

                // Populate the CommandComboBox with card names
                CommandComboBox1.DataSource = cardNames;

                CommandComboBox1.SelectedIndexChanged += CommandComboBox1_SelectedIndexChanged;
            }
            else
            {
                // "Cards" folder doesn't exist, show a message or handle it as needed
                MessageBox.Show("The 'Cards' folder does not exist.");
            }


        }
        private Color GetPlayerColor(int player)
        {
            if (player >= 1 && player <= TotalPlayers)
            {
                return playerColors[player - 1];
            }
            else
            {
                // If the player number is out of range, return a default color (e.g., White)
                return Color.White;
            }
        }


        public void GenerateWifiQRCode(string ssid, string password)
        {
            // Format the Wi-Fi network details according to the Wi-Fi QR Code standard
            string wifiFormat = $"WIFI:S:{ssid};T:WPA;P:{password};;";

            // Create a new instance of the QRCodeGenerator class
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(wifiFormat, QRCodeGenerator.ECCLevel.Q);

            // Create a QR code as a bitmap
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                // Assuming 'WifiConnect' is a PictureBox control in your Windows Forms application
                // Convert the Bitmap to an Image and display it in the PictureBox
                WifiConnect.Image = qrCodeImage;
                WifiConnect.SizeMode = PictureBoxSizeMode.StretchImage;
                WifiConnect.Visible = false;
            }
        }

        //Generic Functions
        private void PoisonDmgBar_MouseClick(object sender, MouseEventArgs e)
        {
            CustomProgressBar clickedBar = sender as CustomProgressBar;
            if (clickedBar == null) return;

            int playerIndex = Array.IndexOf(playerPoisonDmgBars, clickedBar);
            if (playerIndex != -1)
            {
                // Find the corresponding NumericUpDown control for the player's HP
                NumericUpDown playerHpControl = this.Controls.Find($"Player{playerIndex + 1}HP", true).FirstOrDefault() as NumericUpDown;
                if (playerHpControl != null)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (clickedBar.Value < clickedBar.Maximum)
                        {
                            clickedBar.Value += 1;
                            //if (playerHpControl.Value > playerHpControl.Minimum) // Check to avoid going below minimum
                            if (clickedBar.Value == 10)
                            {
                                playerHpControl.Value = 0; // Decrease HP by 1
                            }
                        }
                    }
                    else if (e.Button == MouseButtons.Right && clickedBar.Value > 0)
                    {
                        clickedBar.Value -= 1;
                        if (playerHpControl.Value < playerHpControl.Maximum) // Check to avoid going above maximum
                        {
                            //playerHpControl.Value += 1; // Increase HP by 1
                        }
                    }
                    clickedBar.Invalidate(); // Request the progress bar to repaint
                }
                else
                {
                    //Console.WriteLine($"HP control for player {playerIndex + 1} not found.");
                }
            }
            else
            {
                //Console.WriteLine("Clicked poison progress bar not found in the array.");
            }
        }
        private void UpdateRelatedProgressBarNames(int player, string newName)
        {
            // Update commander damage bars where this player is the opponent
            for (int i = 0; i < TotalPlayers; i++)
            {
                if (playerCommanderDmgBars[i, player - 1] != null)
                {
                    playerCommanderDmgBars[i, player - 1].PlayerName = newName;
                    playerCommanderDmgBars[i, player - 1].Invalidate(); // Refresh the progress bar
                }
            }
        }


        //Commander Damage Click Functions
        private void CommanderDmgBar_MouseClick(object sender, MouseEventArgs e)
        {
            CustomProgressBar clickedBar = sender as CustomProgressBar;
            if (clickedBar == null)
            {
                //Console.WriteLine("Clicked bar is null.");
                return;
            }

            //Console.WriteLine($"Clicked bar name: {clickedBar.Name}, Value: {clickedBar.Value}");

            int playerTakingDamage = ParsePlayerTakingDamageFromBarName(clickedBar.Name);
            //Console.WriteLine($"Player taking damage: {playerTakingDamage}");

            // Adjust to find a NumericUpDown control instead of a ProgressBar
            NumericUpDown hpBar = this.Controls.Find($"Player{playerTakingDamage}HP", true).FirstOrDefault() as NumericUpDown;

            if (hpBar != null)
            {
                //Console.WriteLine($"HP NumericUpDown found: {hpBar.Name}, Current HP: {hpBar.Value}");

                if (e.Button == MouseButtons.Left)
                {
                    if (clickedBar.Value < clickedBar.Maximum)
                    {

                            clickedBar.Value += 1; // Increment the progress bar value
                        hpBar.Value = Math.Max(hpBar.Minimum, hpBar.Value - 1); // Decrement HP, ensuring it doesn't go below the minimum
                    
                    }

                    if (clickedBar.Value == 21)
                    {
                        hpBar.Value = 0;
                    }
                }
                else if (e.Button == MouseButtons.Right && clickedBar.Value > 0)
                {
                    clickedBar.Value -= 1; // Decrement the progress bar value
                    hpBar.Value = Math.Min(hpBar.Maximum, hpBar.Value + 1); // Increment HP, ensuring it doesn't exceed the maximum
                }

                clickedBar.Invalidate(); // Request the progress bar to repaint
            }
            else
            {
                //Console.WriteLine($"HP NumericUpDown for player {playerTakingDamage} not found.");
            }
        }
        private int ParsePlayerTakingDamageFromBarName(string barName)
        {
            var match = Regex.Match(barName, @"P\d+CommanderDMGP(\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                //Console.WriteLine($"Failed to parse player number from {barName}");
                throw new InvalidOperationException("Progress bar name is not in the correct format.");
            }
        }


        //Start new game
        private void InitializeNewGame()
        {

            Treachery1Unveil.Visible = false;
            Treachery2Unveil.Visible = false;
            Treachery3Unveil.Visible = false;
            Treachery4Unveil.Visible = false;
            Treachery5Unveil.Visible = false;
            Treachery6Unveil.Visible = false;
            Treachery7Unveil.Visible = false;
            Treachery8Unveil.Visible = false;


            // Loop through each player
            for (int playerNum = 1; playerNum <= MaxPlayers; playerNum++)
            {
                // Reset poison damage bars
                CustomProgressBar poisonDmgBar = playerPoisonDmgBars[playerNum - 1];
                if (poisonDmgBar != null)
                {
                    poisonDmgBar.Value = 0;
                }

                // Reset commander damage bars
                for (int opponentNum = 1; opponentNum <= MaxPlayers; opponentNum++)
                {
                    if (playerNum != opponentNum)
                    {
                        CustomProgressBar cmdDmgBar = playerCommanderDmgBars[playerNum - 1, opponentNum - 1];
                        if (cmdDmgBar != null)
                        {
                            cmdDmgBar.Value = 0;
                        }
                    }
                }

                NumericUpDown hpHealthBox = this.Controls.Find($"Player{playerNum}HP", true).FirstOrDefault() as NumericUpDown;
                TextBox RoleTextBox = this.Controls.Find($"RolePlayer{playerNum}", true).FirstOrDefault() as TextBox;

                if (RoleTextBox != null)
                {
                    RoleTextBox.Text = "NOT SET";
                    RoleTextBox.Visible = false;
                }

                if (hpHealthBox != null)
                {
                    hpHealthBox.Text = "40";
                }



            }

            Treachery1.Visible = false;
            Treachery2.Visible = false;
            Treachery3.Visible = false;
            Treachery4.Visible = false;
            Treachery5.Visible = false;
            Treachery6.Visible = false;
            Treachery7.Visible = false;
            Treachery8.Visible = false;

        }
        private void NewGame_Click(object sender, EventArgs e)
        {
            InitializeNewGame(); // Call the InitializeNewGame function
        }


        //Caching function for local config files
        private void SavePortToConfigFile(int port)
        {
            string configFilePath = "config.ini";
            try
            {
                File.WriteAllText(configFilePath, $"Port={port}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to write to config file: {ex.Message}", "Error");
            }
        }

        //Fetching and Setting Commander and Treachery Images
        private async void FetchCommander_Click(object sender, EventArgs e)
        {
            // Assuming CommanderSelector is your ComboBox name
            var selectedCommander = CommanderSelector.SelectedItem.ToString();

            // Assuming the naming convention is "CommanderImageX" where X is the player number
            PictureBox correspondingPictureBox = this.Controls.Find(selectedCommander, true).FirstOrDefault() as PictureBox;

            // Fetch the name from the corresponding TextBox. Since there's only one TextBox named "Commander1", we can hardcode it.
            TextBox correspondingTextBox = this.Controls.Find("Commander1", true).FirstOrDefault() as TextBox;

            if (correspondingTextBox != null && correspondingPictureBox != null)
            {
                string cardName = correspondingTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(cardName))
                {
                    try
                    {
                        await FetchAndDisplayCardImage(cardName, correspondingPictureBox); // Call the new method
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a card name.");
                }
            }
        }

        private async Task FetchAndDisplayCardImage(string cardName, PictureBox pictureBox)
        {
            // The code for fetching and displaying the card image goes here
            // You can reuse the code from your FetchAndSaveCardImage method
            await FetchAndSaveCardImage(cardName, pictureBox);
        }
        private async Task FetchAndSaveCardImage(string cardName, PictureBox pictureBox)
        {
            string cardsFolderPath = Path.Combine(Application.StartupPath, "Cards");
            string urlFilePath = Path.Combine(Application.StartupPath, "ImageUrl.txt");
            string errorFilePath = Path.Combine(Application.StartupPath, "ErrorLog.txt");

            try
            {
                if (!Directory.Exists(cardsFolderPath))
                {
                    Directory.CreateDirectory(cardsFolderPath);
                }

                string apiUrl = $"https://api.scryfall.com/cards/named?fuzzy={Uri.EscapeDataString(cardName)}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var cardData = JsonConvert.DeserializeObject<dynamic>(json);

                        string actualCardName = cardData.name;
                        string imageFileName = SanitizeFileName(actualCardName) + ".jpg";
                        string filePath = Path.Combine(cardsFolderPath, imageFileName);
                        string rulingsUri = cardData.rulings_uri;
                        string imageUrl = cardData.image_uris.large;

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            File.WriteAllText(urlFilePath, imageUrl);

                            HttpResponseMessage imageResponse = await client.GetAsync(imageUrl);
                            if (imageResponse.IsSuccessStatusCode)
                            {
                                using (var ms = await imageResponse.Content.ReadAsStreamAsync())
                                {
                                    Bitmap image = new Bitmap(ms);
                                    pictureBox.Image = image;
                                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                    image.Save(filePath);
                                }

                                pictureBox.ImageLocation = filePath;
                            }
                            else
                            {
                                // Handle image download error
                            }
                        }
                        else
                        {
                            // Handle missing image URL
                        }

                        // Fetch and save the rulings
                        if (!string.IsNullOrEmpty(rulingsUri))
                        {
                            HttpResponseMessage rulingsResponse = await client.GetAsync(rulingsUri);
                            if (rulingsResponse.IsSuccessStatusCode)
                            {
                                string rulingsJson = await rulingsResponse.Content.ReadAsStringAsync();
                                var rulingsData = JsonConvert.DeserializeObject<dynamic>(rulingsJson);

                                StringBuilder rulingsText = new StringBuilder();
                                foreach (var ruling in rulingsData.data)
                                {
                                    rulingsText.AppendLine(ruling.comment.ToString());
                                }

                                // Use the same base name as the image file, but with a .txt extension
                                string rulingsFilePath = Path.Combine(cardsFolderPath, SanitizeFileName(actualCardName) + ".txt");
                                File.WriteAllText(rulingsFilePath, rulingsText.ToString());
                            }
                            else
                            {
                                // Handle rulings fetch error
                            }
                        }

                    }
                    else
                    {
                        // Handle API response error
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                File.AppendAllText(errorFilePath, errorMessage + Environment.NewLine);
                //MessageBox.Show(errorMessage + "\nError log saved to: " + errorFilePath);
            }

            pictureBox.Invalidate();
        }
        private string SanitizeFileName(string fileName)
        {
            // Remove invalid characters from filename
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }


        //POP Up functions for Commanders and Treachery Cards
        private void InitializeImagePopupForm()
        {
            // Create the pop-up form
            imagePopupForm = new Form();

            // Set form properties
            imagePopupForm.FormBorderStyle = FormBorderStyle.None;
            imagePopupForm.StartPosition = FormStartPosition.Manual;
            imagePopupForm.Size = new Size(360, 500); // Adjust size as needed
            imagePopupForm.BackColor = Color.Black; // Set the form's background color to black
            //imagePopupForm.TransparencyKey = Color.Black; // Set TransparencyKey to the same color

            // Create a PictureBox for displaying images in the pop-up
            PictureBox popupPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black, // Set the PictureBox background color to black
                BorderStyle = BorderStyle.None, // Set the border style to a single black line
            };
            imagePopupForm.Controls.Add(popupPictureBox);

            // Subscribe each PictureBox to the MouseEnter and MouseLeave events
            Treachery1.MouseEnter += (sender, e) => ShowImagePopup(Treachery1);
            Treachery1.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery2.MouseEnter += (sender, e) => ShowImagePopup(Treachery2);
            Treachery2.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery3.MouseEnter += (sender, e) => ShowImagePopup(Treachery3);
            Treachery3.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery4.MouseEnter += (sender, e) => ShowImagePopup(Treachery4);
            Treachery4.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery5.MouseEnter += (sender, e) => ShowImagePopup(Treachery5);
            Treachery5.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery6.MouseEnter += (sender, e) => ShowImagePopup(Treachery6);
            Treachery6.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery7.MouseEnter += (sender, e) => ShowImagePopup(Treachery7);
            Treachery7.MouseLeave += (sender, e) => imagePopupForm.Hide();

            Treachery8.MouseEnter += (sender, e) => ShowImagePopup(Treachery8);
            Treachery8.MouseLeave += (sender, e) => imagePopupForm.Hide();


            CommanderImage1.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage1);
            CommanderImage1.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage2.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage2);
            CommanderImage2.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage3.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage3);
            CommanderImage3.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage4.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage4);
            CommanderImage4.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage5.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage5);
            CommanderImage5.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage6.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage6);
            CommanderImage6.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage7.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage7);
            CommanderImage7.MouseLeave += (sender, e) => imagePopupForm.Hide();

            CommanderImage8.MouseEnter += (sender, e) => ShowImagePopup(CommanderImage8);
            CommanderImage8.MouseLeave += (sender, e) => imagePopupForm.Hide();

            // ...
        }
        private void ShowImagePopup(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                PictureBox popupPictureBox = imagePopupForm.Controls[0] as PictureBox;
                popupPictureBox.Image = pictureBox.Image;

                string imageFileName = Path.GetFileNameWithoutExtension(pictureBox.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", imageFileName + ".txt");

                Point locationOnScreen = pictureBox.PointToScreen(Point.Empty);
                int popupHeight = imagePopupForm.Height;

                // Calculate the Y-coordinate for alignment
                int yOffset = 0;
                if (pictureBox == Treachery1 || pictureBox == Treachery2 || pictureBox == Treachery3)
                {
                    // Align with the bottom for Treachery images
                    yOffset = pictureBox.Height - popupHeight;
                }
                if (pictureBox == Treachery4 || pictureBox == Treachery5 || pictureBox == Treachery6)
                {
                    // Align with the bottom for Treachery images
                    yOffset = pictureBox.Height - popupHeight;
                }
                if (pictureBox == Treachery7 || pictureBox == Treachery8)
                {
                    // Align with the bottom for Treachery images
                    yOffset = pictureBox.Height - popupHeight;
                }



                imagePopupForm.Location = new Point(locationOnScreen.X + pictureBox.Width, locationOnScreen.Y + yOffset);
                imagePopupForm.Show();
            }
        }
        private void ShowRulings(string rulingsFilePath)
        {
            if (File.Exists(rulingsFilePath))
            {
                string rulingsText = File.ReadAllText(rulingsFilePath);

                Form rulingsForm = new Form
                {
                    Size = new Size(400, 300) // Adjust size as needed
                };

                TextBox rulingsTextBox = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                    Text = rulingsText
                };

                rulingsForm.Controls.Add(rulingsTextBox);
                rulingsForm.Show();
            }
            else
            {
                MessageBox.Show("Rulings file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Commander and Treachery Image Functions
        private void FetchCommander1_Click(object sender, EventArgs e)
        {
            FetchCommander_Click(sender, e);
        }
        private void CommandComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if any item is selected in CommanderSelector
            if (CommanderSelector.SelectedItem != null)
            {
                // Get the selected card name from the combobox
                string selectedCardName = CommandComboBox1.SelectedItem as string;

                // Get the corresponding PictureBox based on the selected item in CommanderSelector
                string pictureBoxName = CommanderSelector.SelectedItem.ToString().Replace("CommanderImage", "CommanderImage");
                PictureBox selectedPictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;

                // Update the corresponding PictureBox with the selected card image
                if (selectedPictureBox != null && !string.IsNullOrWhiteSpace(selectedCardName))
                {
                    _ = FetchAndDisplayCardImage(selectedCardName, selectedPictureBox); // Suppress the warning
                }
            }
        }




        private void CommanderImage1_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage1.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage2_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage2.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage3_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage3.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage4_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage4.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage5_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage5.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage6_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage6.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage7_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage7.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void CommanderImage8_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                // Get the path to the rules file for CommanderImage1
                string commanderName = Path.GetFileNameWithoutExtension(CommanderImage8.ImageLocation);
                string rulesFilePath = Path.Combine(Application.StartupPath, "Cards", commanderName + ".txt");

                try
                {
                    if (File.Exists(rulesFilePath))
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox();
                        rulesTextBox.Multiline = true;
                        rulesTextBox.ReadOnly = true;
                        rulesTextBox.ScrollBars = ScrollBars.Vertical;
                        rulesTextBox.Dock = DockStyle.Fill;
                        rulesTextBox.BackColor = Color.Black; // Background color
                        rulesTextBox.ForeColor = Color.White; // Text color
                        rulesTextBox.Font = new Font("Arial", 10); // Font and size
                        rulesTextBox.Text = File.ReadAllText(rulesFilePath);

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this commander.", "Commander Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Commander Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        //Pop up for Treachery Rulings
        private void Treachery1_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule1 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule1
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery3_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule3 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule3
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery6_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule6 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule6
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery8_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule8 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule8
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery5_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule5 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule5
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery4_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule4 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule4
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void Treachery2_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule2 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule2
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void Treachery7_Click(object sender, EventArgs e)
        {
            if (rulesForm == null || rulesForm.IsDisposed)
            {
                try
                {
                    if (TreacheryRule7 != null)
                    {
                        // Create a new form to display the rules
                        rulesForm = new Form();
                        rulesForm.Size = new Size(400, 300); // Adjust size as needed

                        // Create a TextBox to display the rules text
                        TextBox rulesTextBox = new TextBox
                        {
                            Multiline = true,
                            ReadOnly = true,
                            ScrollBars = ScrollBars.Vertical,
                            Dock = DockStyle.Fill,
                            BackColor = Color.Black, // Background color
                            ForeColor = Color.White, // Text color
                            Font = new Font("Arial", 10), // Font and size
                            Text = TreacheryRule7
                        };

                        // Add the TextBox to the form
                        rulesForm.Controls.Add(rulesTextBox);

                        // Add minimize, maximize, and close buttons
                        rulesForm.MinimizeBox = true;
                        rulesForm.MaximizeBox = true;
                        rulesForm.ControlBox = true;

                        // Show the rules form
                        rulesForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("No rules available for this Role.", "Treachery Role Rule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading rule: " + ex.Message, "Treachery Role Rule Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        //AOEDamage Functions
        private void Player1AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player2AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player3AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player4AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player5AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player6AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player7AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void Player8AOEDMG_Click(object sender, EventArgs e)
        {
            AOEDamage_Click(sender, e);
        }
        private void AOEDamage_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                // Determine the player triggering the AOE from the button's name
                int triggeringPlayerNumber = ParsePlayerNumberFromAoEButtonName(button.Name);

                // Apply AOE damage to all other players
                for (int playerNum = 1; playerNum <= 8; playerNum++)
                {
                    if (playerNum != triggeringPlayerNumber)
                    {
                        // Get the corresponding NumericUpDown control for the player's HP
                        NumericUpDown hpControl = this.Controls.Find($"Player{playerNum}HP", true).FirstOrDefault() as NumericUpDown;

                        if (hpControl != null)
                        {
                            // Apply AOE damage
                            int currentHP = (int)hpControl.Value;
                            currentHP -= 1;

                            // Ensure HP doesn't go below minimum (optional)
                            currentHP = Math.Max(currentHP, (int)hpControl.Minimum);

                            // Update the NumericUpDown with the new value
                            hpControl.Value = currentHP;
                        }
                        else
                        {
                            // MessageBox.Show($"HP control for player {playerNum} not found.");
                        }
                    }
                }
            }
        }
        private int ParsePlayerNumberFromAoEButtonName(string buttonName)
        {
            // Extract player number from the button's name
            // Example: "Player1AOEDMG" => playerNumber = 1
            var match = Regex.Match(buttonName, @"Player(\d+)AOEDMG");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            throw new InvalidOperationException("Button name is not in the correct format.");
        }

        //Unveil Functions
        public void Treachery1Unveil_Click(object sender, EventArgs e)
        {
            Treachery1.Visible = !Treachery1.Visible;
            RolePlayer1.Visible = !RolePlayer1.Visible;
        }
        public void Treachery2Unveil_Click(object sender, EventArgs e)
        {
            Treachery2.Visible = !Treachery2.Visible;
            RolePlayer2.Visible = !RolePlayer2.Visible;
        }
        public void Treachery3Unveil_Click(object sender, EventArgs e)
        {
            Treachery3.Visible = !Treachery3.Visible;
            RolePlayer3.Visible = !RolePlayer3.Visible;
        }
        public void Treachery4Unveil_Click(object sender, EventArgs e)
        {
            Treachery4.Visible = !Treachery4.Visible;
            RolePlayer4.Visible = !RolePlayer4.Visible;
            Treachery4.Refresh();
        }
        public void Treachery5Unveil_Click(object sender, EventArgs e)
        {
            Treachery5.Visible = !Treachery5.Visible;
            RolePlayer5.Visible = !RolePlayer5.Visible;
        }
        public void Treachery6Unveil_Click(object sender, EventArgs e)
        {
            Treachery6.Visible = !Treachery6.Visible;
            RolePlayer6.Visible = !RolePlayer6.Visible;
        }
        public void Treachery7Unveil_Click(object sender, EventArgs e)
        {
            Treachery7.Visible = !Treachery7.Visible;
            RolePlayer7.Visible = !RolePlayer7.Visible;
        }
        public void Treachery8Unveil_Click(object sender, EventArgs e)
        {
            Treachery8.Visible = !Treachery8.Visible;
            RolePlayer8.Visible = !RolePlayer8.Visible;
        }


        //Player Selection
        private void SetPlayersTo2_Click(object sender, EventArgs e)
        {
            selectedPlayers = 2;

            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = false;
            P4CommanderDMGP1.Visible = false;
            P5CommanderDMGP1.Visible = false;
            P6CommanderDMGP1.Visible = false;
            P7CommanderDMGP1.Visible = false;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = false;
            P4CommanderDMGP2.Visible = false;
            P5CommanderDMGP2.Visible = false;
            P6CommanderDMGP2.Visible = false;
            P7CommanderDMGP2.Visible = false;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = false;
            Player3HP.Visible = false;
            CommanderImage3.Visible = false;
            Player3AOEDMG.Visible = false;
            Player3PoisonDMG.Visible = false;
            P1CommanderDMGP3.Visible = false;
            P2CommanderDMGP3.Visible = false;
            P4CommanderDMGP3.Visible = false;
            P5CommanderDMGP3.Visible = false;
            P6CommanderDMGP3.Visible = false;
            P7CommanderDMGP3.Visible = false;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = false;
            Player4HP.Visible = false;
            CommanderImage4.Visible = false;
            Player4AOEDMG.Visible = false;
            Player4PoisonDMG.Visible = false;
            P1CommanderDMGP4.Visible = false;
            P2CommanderDMGP4.Visible = false;
            P3CommanderDMGP4.Visible = false;
            P5CommanderDMGP4.Visible = false;
            P6CommanderDMGP4.Visible = false;
            P7CommanderDMGP4.Visible = false;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = false;
            Player5HP.Visible = false;
            CommanderImage5.Visible = false;
            Player5AOEDMG.Visible = false;
            Player5PoisonDMG.Visible = false;
            P1CommanderDMGP5.Visible = false;
            P2CommanderDMGP5.Visible = false;
            P3CommanderDMGP5.Visible = false;
            P4CommanderDMGP5.Visible = false;
            P6CommanderDMGP5.Visible = false;
            P7CommanderDMGP5.Visible = false;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = false;
            Player6HP.Visible = false;
            CommanderImage6.Visible = false;
            Player6AOEDMG.Visible = false;
            Player6PoisonDMG.Visible = false;
            P1CommanderDMGP6.Visible = false;
            P2CommanderDMGP6.Visible = false;
            P3CommanderDMGP6.Visible = false;
            P4CommanderDMGP6.Visible = false;
            P5CommanderDMGP6.Visible = false;
            P7CommanderDMGP6.Visible = false;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = false;
            Player7HP.Visible = false;
            CommanderImage7.Visible = false;
            Player7AOEDMG.Visible = false;
            Player7PoisonDMG.Visible = false;
            P1CommanderDMGP7.Visible = false;
            P2CommanderDMGP7.Visible = false;
            P3CommanderDMGP7.Visible = false;
            P4CommanderDMGP7.Visible = false;
            P5CommanderDMGP7.Visible = false;
            P6CommanderDMGP7.Visible = false;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;


        }
        private void SetPlayersTo3_Click(object sender, EventArgs e)
        {
            selectedPlayers = 3;

            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = false;
            P5CommanderDMGP1.Visible = false;
            P6CommanderDMGP1.Visible = false;
            P7CommanderDMGP1.Visible = false;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = false;
            P5CommanderDMGP2.Visible = false;
            P6CommanderDMGP2.Visible = false;
            P7CommanderDMGP2.Visible = false;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = false;
            P5CommanderDMGP3.Visible = false;
            P6CommanderDMGP3.Visible = false;
            P7CommanderDMGP3.Visible = false;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = false;
            Player4HP.Visible = false;
            CommanderImage4.Visible = false;
            Player4AOEDMG.Visible = false;
            Player4PoisonDMG.Visible = false;
            P1CommanderDMGP4.Visible = false;
            P2CommanderDMGP4.Visible = false;
            P3CommanderDMGP4.Visible = false;
            P5CommanderDMGP4.Visible = false;
            P6CommanderDMGP4.Visible = false;
            P7CommanderDMGP4.Visible = false;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = false;
            Player5HP.Visible = false;
            CommanderImage5.Visible = false;
            Player5AOEDMG.Visible = false;
            Player5PoisonDMG.Visible = false;
            P1CommanderDMGP5.Visible = false;
            P2CommanderDMGP5.Visible = false;
            P3CommanderDMGP5.Visible = false;
            P4CommanderDMGP5.Visible = false;
            P6CommanderDMGP5.Visible = false;
            P7CommanderDMGP5.Visible = false;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = false;
            Player6HP.Visible = false;
            CommanderImage6.Visible = false;
            Player6AOEDMG.Visible = false;
            Player6PoisonDMG.Visible = false;
            P1CommanderDMGP6.Visible = false;
            P2CommanderDMGP6.Visible = false;
            P3CommanderDMGP6.Visible = false;
            P4CommanderDMGP6.Visible = false;
            P5CommanderDMGP6.Visible = false;
            P7CommanderDMGP6.Visible = false;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = false;
            Player7HP.Visible = false;
            CommanderImage7.Visible = false;
            Player7AOEDMG.Visible = false;
            Player7PoisonDMG.Visible = false;
            P1CommanderDMGP7.Visible = false;
            P2CommanderDMGP7.Visible = false;
            P3CommanderDMGP7.Visible = false;
            P4CommanderDMGP7.Visible = false;
            P5CommanderDMGP7.Visible = false;
            P6CommanderDMGP7.Visible = false;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;


        }
        private void SetPlayersTo4_Click(object sender, EventArgs e)
        {
            selectedPlayers = 4;

            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = true;
            P5CommanderDMGP1.Visible = false;
            P6CommanderDMGP1.Visible = false;
            P7CommanderDMGP1.Visible = false;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = true;
            P5CommanderDMGP2.Visible = false;
            P6CommanderDMGP2.Visible = false;
            P7CommanderDMGP2.Visible = false;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = true;
            P5CommanderDMGP3.Visible = false;
            P6CommanderDMGP3.Visible = false;
            P7CommanderDMGP3.Visible = false;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = true;
            Player4HP.Visible = true;
            CommanderImage4.Visible = true;
            Player4AOEDMG.Visible = true;
            Player4PoisonDMG.Visible = true;
            P1CommanderDMGP4.Visible = true;
            P2CommanderDMGP4.Visible = true;
            P3CommanderDMGP4.Visible = true;
            P5CommanderDMGP4.Visible = false;
            P6CommanderDMGP4.Visible = false;
            P7CommanderDMGP4.Visible = false;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = false;
            Player5HP.Visible = false;
            CommanderImage5.Visible = false;
            Player5AOEDMG.Visible = false;
            Player5PoisonDMG.Visible = false;
            P1CommanderDMGP5.Visible = false;
            P2CommanderDMGP5.Visible = false;
            P3CommanderDMGP5.Visible = false;
            P4CommanderDMGP5.Visible = false;
            P6CommanderDMGP5.Visible = false;
            P7CommanderDMGP5.Visible = false;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = false;
            Player6HP.Visible = false;
            CommanderImage6.Visible = false;
            Player6AOEDMG.Visible = false;
            Player6PoisonDMG.Visible = false;
            P1CommanderDMGP6.Visible = false;
            P2CommanderDMGP6.Visible = false;
            P3CommanderDMGP6.Visible = false;
            P4CommanderDMGP6.Visible = false;
            P5CommanderDMGP6.Visible = false;
            P7CommanderDMGP6.Visible = false;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = false;
            Player7HP.Visible = false;
            CommanderImage7.Visible = false;
            Player7AOEDMG.Visible = false;
            Player7PoisonDMG.Visible = false;
            P1CommanderDMGP7.Visible = false;
            P2CommanderDMGP7.Visible = false;
            P3CommanderDMGP7.Visible = false;
            P4CommanderDMGP7.Visible = false;
            P5CommanderDMGP7.Visible = false;
            P6CommanderDMGP7.Visible = false;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;



        }
        private void SetPlayersTo5_Click(object sender, EventArgs e)
        {
            selectedPlayers = 5;

            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = true;
            P5CommanderDMGP1.Visible = true;
            P6CommanderDMGP1.Visible = false;
            P7CommanderDMGP1.Visible = false;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = true;
            P5CommanderDMGP2.Visible = true;
            P6CommanderDMGP2.Visible = false;
            P7CommanderDMGP2.Visible = false;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = true;
            P5CommanderDMGP3.Visible = true;
            P6CommanderDMGP3.Visible = false;
            P7CommanderDMGP3.Visible = false;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = true;
            Player4HP.Visible = true;
            CommanderImage4.Visible = true;
            Player4AOEDMG.Visible = true;
            Player4PoisonDMG.Visible = true;
            P1CommanderDMGP4.Visible = true;
            P2CommanderDMGP4.Visible = true;
            P3CommanderDMGP4.Visible = true;
            P5CommanderDMGP4.Visible = true;
            P6CommanderDMGP4.Visible = false;
            P7CommanderDMGP4.Visible = false;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = true;
            Player5HP.Visible = true;
            CommanderImage5.Visible = true;
            Player5AOEDMG.Visible = true;
            Player5PoisonDMG.Visible = true;
            P1CommanderDMGP5.Visible = true;
            P2CommanderDMGP5.Visible = true;
            P3CommanderDMGP5.Visible = true;
            P4CommanderDMGP5.Visible = true;
            P6CommanderDMGP5.Visible = false;
            P7CommanderDMGP5.Visible = false;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = false;
            Player6HP.Visible = false;
            CommanderImage6.Visible = false;
            Player6AOEDMG.Visible = false;
            Player6PoisonDMG.Visible = false;
            P1CommanderDMGP6.Visible = false;
            P2CommanderDMGP6.Visible = false;
            P3CommanderDMGP6.Visible = false;
            P4CommanderDMGP6.Visible = false;
            P5CommanderDMGP6.Visible = false;
            P7CommanderDMGP6.Visible = false;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = false;
            Player7HP.Visible = false;
            CommanderImage7.Visible = false;
            Player7AOEDMG.Visible = false;
            Player7PoisonDMG.Visible = false;
            P1CommanderDMGP7.Visible = false;
            P2CommanderDMGP7.Visible = false;
            P3CommanderDMGP7.Visible = false;
            P4CommanderDMGP7.Visible = false;
            P5CommanderDMGP7.Visible = false;
            P6CommanderDMGP7.Visible = false;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;


        }
        private void SetPlayersTo6_Click(object sender, EventArgs e)
        {
            selectedPlayers = 6;


            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = true;
            P5CommanderDMGP1.Visible = true;
            P6CommanderDMGP1.Visible = true;
            P7CommanderDMGP1.Visible = false;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = true;
            P5CommanderDMGP2.Visible = true;
            P6CommanderDMGP2.Visible = true;
            P7CommanderDMGP2.Visible = false;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = true;
            P5CommanderDMGP3.Visible = true;
            P6CommanderDMGP3.Visible = true;
            P7CommanderDMGP3.Visible = false;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = true;
            Player4HP.Visible = true;
            CommanderImage4.Visible = true;
            Player4AOEDMG.Visible = true;
            Player4PoisonDMG.Visible = true;
            P1CommanderDMGP4.Visible = true;
            P2CommanderDMGP4.Visible = true;
            P3CommanderDMGP4.Visible = true;
            P5CommanderDMGP4.Visible = true;
            P6CommanderDMGP4.Visible = true;
            P7CommanderDMGP4.Visible = false;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = true;
            Player5HP.Visible = true;
            CommanderImage5.Visible = true;
            Player5AOEDMG.Visible = true;
            Player5PoisonDMG.Visible = true;
            P1CommanderDMGP5.Visible = true;
            P2CommanderDMGP5.Visible = true;
            P3CommanderDMGP5.Visible = true;
            P4CommanderDMGP5.Visible = true;
            P6CommanderDMGP5.Visible = true;
            P7CommanderDMGP5.Visible = false;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = true;
            Player6HP.Visible = true;
            CommanderImage6.Visible = true;
            Player6AOEDMG.Visible = true;
            Player6PoisonDMG.Visible = true;
            P1CommanderDMGP6.Visible = true;
            P2CommanderDMGP6.Visible = true;
            P3CommanderDMGP6.Visible = true;
            P4CommanderDMGP6.Visible = true;
            P5CommanderDMGP6.Visible = true;
            P7CommanderDMGP6.Visible = false;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = false;
            Player7HP.Visible = false;
            CommanderImage7.Visible = false;
            Player7AOEDMG.Visible = false;
            Player7PoisonDMG.Visible = false;
            P1CommanderDMGP7.Visible = false;
            P2CommanderDMGP7.Visible = false;
            P3CommanderDMGP7.Visible = false;
            P4CommanderDMGP7.Visible = false;
            P5CommanderDMGP7.Visible = false;
            P6CommanderDMGP7.Visible = false;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;



        }
        private void SetPlayersTo7_Click(object sender, EventArgs e)
        {
            selectedPlayers = 7;


            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = true;
            P5CommanderDMGP1.Visible = true;
            P6CommanderDMGP1.Visible = true;
            P7CommanderDMGP1.Visible = true;
            P8CommanderDMGP1.Visible = false;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = true;
            P5CommanderDMGP2.Visible = true;
            P6CommanderDMGP2.Visible = true;
            P7CommanderDMGP2.Visible = true;
            P8CommanderDMGP2.Visible = false;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = true;
            P5CommanderDMGP3.Visible = true;
            P6CommanderDMGP3.Visible = true;
            P7CommanderDMGP3.Visible = true;
            P8CommanderDMGP3.Visible = false;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = true;
            Player4HP.Visible = true;
            CommanderImage4.Visible = true;
            Player4AOEDMG.Visible = true;
            Player4PoisonDMG.Visible = true;
            P1CommanderDMGP4.Visible = true;
            P2CommanderDMGP4.Visible = true;
            P3CommanderDMGP4.Visible = true;
            P5CommanderDMGP4.Visible = true;
            P6CommanderDMGP4.Visible = true;
            P7CommanderDMGP4.Visible = true;
            P8CommanderDMGP4.Visible = false;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = true;
            Player5HP.Visible = true;
            CommanderImage5.Visible = true;
            Player5AOEDMG.Visible = true;
            Player5PoisonDMG.Visible = true;
            P1CommanderDMGP5.Visible = true;
            P2CommanderDMGP5.Visible = true;
            P3CommanderDMGP5.Visible = true;
            P4CommanderDMGP5.Visible = true;
            P6CommanderDMGP5.Visible = true;
            P7CommanderDMGP5.Visible = true;
            P8CommanderDMGP5.Visible = false;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = true;
            Player6HP.Visible = true;
            CommanderImage6.Visible = true;
            Player6AOEDMG.Visible = true;
            Player6PoisonDMG.Visible = true;
            P1CommanderDMGP6.Visible = true;
            P2CommanderDMGP6.Visible = true;
            P3CommanderDMGP6.Visible = true;
            P4CommanderDMGP6.Visible = true;
            P5CommanderDMGP6.Visible = true;
            P7CommanderDMGP6.Visible = true;
            P8CommanderDMGP6.Visible = false;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = true;
            Player7HP.Visible = true;
            CommanderImage7.Visible = true;
            Player7AOEDMG.Visible = true;
            Player7PoisonDMG.Visible = true;
            P1CommanderDMGP7.Visible = true;
            P2CommanderDMGP7.Visible = true;
            P3CommanderDMGP7.Visible = true;
            P4CommanderDMGP7.Visible = true;
            P5CommanderDMGP7.Visible = true;
            P6CommanderDMGP7.Visible = true;
            P8CommanderDMGP7.Visible = false;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = false;
            Player8HP.Visible = false;
            CommanderImage8.Visible = false;
            Player8AOEDMG.Visible = false;
            Player8PoisonDMG.Visible = false;
            P1CommanderDMGP8.Visible = false;
            P2CommanderDMGP8.Visible = false;
            P3CommanderDMGP8.Visible = false;
            P4CommanderDMGP8.Visible = false;
            P5CommanderDMGP8.Visible = false;
            P6CommanderDMGP8.Visible = false;
            P7CommanderDMGP8.Visible = false;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;


        }
        private void SetPlayersTo8_Click(object sender, EventArgs e)
        {
            selectedPlayers = 8;


            // Code for Player 1
            Player1.Visible = true;
            Player1HP.Visible = true;
            CommanderImage1.Visible = true;
            Player1AOEDMG.Visible = true;
            Player1PoisonDMG.Visible = true;
            P2CommanderDMGP1.Visible = true;
            P3CommanderDMGP1.Visible = true;
            P4CommanderDMGP1.Visible = true;
            P5CommanderDMGP1.Visible = true;
            P6CommanderDMGP1.Visible = true;
            P7CommanderDMGP1.Visible = true;
            P8CommanderDMGP1.Visible = true;
            RolePlayer1.Visible = false;
            Treachery1.Visible = false;
            Treachery1Unveil.Visible = false;


            // Code for Player 2
            Player2.Visible = true;
            Player2HP.Visible = true;
            CommanderImage2.Visible = true;
            Player2AOEDMG.Visible = true;
            Player2PoisonDMG.Visible = true;
            P1CommanderDMGP2.Visible = true;
            P3CommanderDMGP2.Visible = true;
            P4CommanderDMGP2.Visible = true;
            P5CommanderDMGP2.Visible = true;
            P6CommanderDMGP2.Visible = true;
            P7CommanderDMGP2.Visible = true;
            P8CommanderDMGP2.Visible = true;
            RolePlayer2.Visible = false;
            Treachery2.Visible = false;
            Treachery2Unveil.Visible = false;

            // Code for Player 3
            Player3.Visible = true;
            Player3HP.Visible = true;
            CommanderImage3.Visible = true;
            Player3AOEDMG.Visible = true;
            Player3PoisonDMG.Visible = true;
            P1CommanderDMGP3.Visible = true;
            P2CommanderDMGP3.Visible = true;
            P4CommanderDMGP3.Visible = true;
            P5CommanderDMGP3.Visible = true;
            P6CommanderDMGP3.Visible = true;
            P7CommanderDMGP3.Visible = true;
            P8CommanderDMGP3.Visible = true;
            RolePlayer3.Visible = false;
            Treachery3.Visible = false;
            Treachery3Unveil.Visible = false;

            // Code for Player 4
            Player4.Visible = true;
            Player4HP.Visible = true;
            CommanderImage4.Visible = true;
            Player4AOEDMG.Visible = true;
            Player4PoisonDMG.Visible = true;
            P1CommanderDMGP4.Visible = true;
            P2CommanderDMGP4.Visible = true;
            P3CommanderDMGP4.Visible = true;
            P5CommanderDMGP4.Visible = true;
            P6CommanderDMGP4.Visible = true;
            P7CommanderDMGP4.Visible = true;
            P8CommanderDMGP4.Visible = true;
            RolePlayer4.Visible = false;
            Treachery4.Visible = false;
            Treachery4Unveil.Visible = false;

            // Code for Player 5
            Player5.Visible = true;
            Player5HP.Visible = true;
            CommanderImage5.Visible = true;
            Player5AOEDMG.Visible = true;
            Player5PoisonDMG.Visible = true;
            P1CommanderDMGP5.Visible = true;
            P2CommanderDMGP5.Visible = true;
            P3CommanderDMGP5.Visible = true;
            P4CommanderDMGP5.Visible = true;
            P6CommanderDMGP5.Visible = true;
            P7CommanderDMGP5.Visible = true;
            P8CommanderDMGP5.Visible = true;
            RolePlayer5.Visible = false;
            Treachery5.Visible = false;
            Treachery5Unveil.Visible = false;

            // Code for Player 6
            Player6.Visible = true;
            Player6HP.Visible = true;
            CommanderImage6.Visible = true;
            Player6AOEDMG.Visible = true;
            Player6PoisonDMG.Visible = true;
            P1CommanderDMGP6.Visible = true;
            P2CommanderDMGP6.Visible = true;
            P3CommanderDMGP6.Visible = true;
            P4CommanderDMGP6.Visible = true;
            P5CommanderDMGP6.Visible = true;
            P7CommanderDMGP6.Visible = true;
            P8CommanderDMGP6.Visible = true;
            RolePlayer6.Visible = false;
            Treachery6.Visible = false;
            Treachery6Unveil.Visible = false;

            // Code for Player 7
            Player7.Visible = true;
            Player7HP.Visible = true;
            CommanderImage7.Visible = true;
            Player7AOEDMG.Visible = true;
            Player7PoisonDMG.Visible = true;
            P1CommanderDMGP7.Visible = true;
            P2CommanderDMGP7.Visible = true;
            P3CommanderDMGP7.Visible = true;
            P4CommanderDMGP7.Visible = true;
            P5CommanderDMGP7.Visible = true;
            P6CommanderDMGP7.Visible = true;
            P8CommanderDMGP7.Visible = true;
            RolePlayer7.Visible = false;
            Treachery7.Visible = false;
            Treachery7Unveil.Visible = false;

            // Code for Player 8
            Player8.Visible = true;
            Player8HP.Visible = true;
            CommanderImage8.Visible = true;
            Player8AOEDMG.Visible = true;
            Player8PoisonDMG.Visible = true;
            P1CommanderDMGP8.Visible = true;
            P2CommanderDMGP8.Visible = true;
            P3CommanderDMGP8.Visible = true;
            P4CommanderDMGP8.Visible = true;
            P5CommanderDMGP8.Visible = true;
            P6CommanderDMGP8.Visible = true;
            P7CommanderDMGP8.Visible = true;
            RolePlayer8.Visible = false;
            Treachery8.Visible = false;
            Treachery8Unveil.Visible = false;


        }




        //Role Text Change (Not in use)
        private void RolePlayer1_TextChanged(object sender, EventArgs e)
        {

        }
        private void RolePlayer2_TextChanged(object sender, EventArgs e)
        {

        }
        private void RolePlayer3_TextChanged(object sender, EventArgs e)
        {

        }

        //Start TreacheryGame
        private void RefreshTreacheryImages()
        {
            for (int i = 1; i <= 8; i++)
            {
                PictureBox treacheryBox = this.Controls.Find($"Treachery{i}", true).FirstOrDefault() as PictureBox;
                treacheryBox?.Refresh(); // Force refresh
            }
        }
        private async void StartTreachery_Click(object sender, EventArgs e)
        {

            //Treachery1Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery2Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery3Unveil_Click(Treachery3Unveil, EventArgs.Empty);
            //Treachery4Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery5Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery6Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery7Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //Treachery8Unveil_Click(Treachery1Unveil, EventArgs.Empty);
            //await Task.Delay(1000); // Wait for 1 second
            StartTreacheryGame();

            // Get the number of selected players from somewhere in your code
            int numberOfPlayers = selectedPlayers;


        }
        private PlayerModel CreatePlayerModel(int playerId)
        {
            // Populate the PlayerModel based on your form's controls
            // This is a placeholder - you'll need to replace it with actual logic
            return new PlayerModel
            {
                Id = playerId,
                // Assign other properties based on your UI elements
            };
        }
        private void StartTreacheryGame()
        {
            // Assume selectedPlayers is the count of players in the game (set somewhere in your form)
            int playerCount = selectedPlayers; // Example: 4, 5, 6, 7, or 8 players

            // Set TreacheryUnveil visibility based on the player count
            for (int i = 1; i <= 8; i++)
            {
                Control treacheryUnveil = this.Controls.Find($"Treachery{i}Unveil", true).FirstOrDefault();
                if (treacheryUnveil != null)
                {
                    treacheryUnveil.Visible = i <= playerCount;
                }
            }

            // Reset Treachery and RolePlayer visibility
            for (int i = 1; i <= 8; i++)
            {
                Control treachery = this.Controls.Find($"Treachery{i}", true).FirstOrDefault();
                if (treachery != null) treachery.Visible = false;

                Control rolePlayer = this.Controls.Find($"RolePlayer{i}", true).FirstOrDefault();
                if (rolePlayer != null) rolePlayer.Visible = false;
            }

            // Load card images into respective lists
            string cardFolder = Path.Combine(Environment.CurrentDirectory, "Treachery");
            List<string> leaders = Directory.GetFiles(cardFolder, "*Leader*.jpg").ToList();
            List<string> traitors = Directory.GetFiles(cardFolder, "*Traitor*.jpg").ToList();
            List<string> assassins = Directory.GetFiles(cardFolder, "*Assassin*.jpg").ToList();
            List<string> guardians = Directory.GetFiles(cardFolder, "*Guardian*.jpg").ToList();

            // Shuffle each list
            Shuffle(leaders);
            Shuffle(traitors);
            Shuffle(assassins);
            Shuffle(guardians);

            // Distribute roles based on the player count
            List<string> allRoles = new List<string>();
            allRoles.Add(leaders[0]);
            allRoles.Add(traitors[0]);
            allRoles.AddRange(assassins.Take(playerCount >= 6 ? 3 : 2));
            if (playerCount >= 5) allRoles.Add(guardians[0]);
            if (playerCount >= 7) allRoles.Add(guardians[1]);
            if (playerCount == 8) allRoles.Add(traitors[1]);
            Shuffle(allRoles);

            // Assign roles to players and save player data
            for (int playerNum = 1; playerNum <= playerCount; playerNum++)
            {
                if (allRoles.Count > 0)
                {
                    string role = allRoles[0];
                    allRoles.RemoveAt(0);

                    PictureBox treacheryBox = this.Controls.Find($"Treachery{playerNum}", true).FirstOrDefault() as PictureBox;
                    if (treacheryBox != null)
                    {
                        // Access the Handle property to ensure the control is fully initialized
                        var handle = treacheryBox.Handle;

                        treacheryBox.SizeMode = PictureBoxSizeMode.StretchImage;

                        string cardPath = Path.Combine(cardFolder, role + (role.EndsWith(".jpg") ? "" : ".jpg"));
                        try
                        {
                            Image newImage = Image.FromFile(cardPath);
                            if (treacheryBox.Image != null)
                            {
                                var oldImage = treacheryBox.Image;
                                treacheryBox.Image = newImage;
                                oldImage.Dispose();
                            }
                            else
                            {
                                treacheryBox.Image = newImage;
                            }

                            treacheryBox.Visible = false; // Set based on your logic
                            treacheryBox.Refresh();
                        }
                        catch (Exception ex)
                        {
                           // Console.WriteLine($"Failed to load image from {cardPath}. Error: {ex.Message}");
                        }

                        // Retrieve data for the current player
                        string playerName = Controls[$"Player{playerNum}"].Text;
                        string treacheryImagePath = cardPath;
                        bool treacheryVisible = false; // Set based on your logic
                        string password = ""; // You mentioned that the password is not set within the form, so set it to an empty string for now

                        // Set the RolePlayerX textbox
                        TextBox roleTextBox = this.Controls.Find($"RolePlayer{playerNum}", true).FirstOrDefault() as TextBox;
                        if (roleTextBox != null)
                        {
                            string roleType = Path.GetFileNameWithoutExtension(role); // This gets the filename without the '.jpg' extension
                            int dashIndex = roleType.IndexOf('-');
                            if (dashIndex >= 0)
                            {
                                roleType = roleType.Substring(0, dashIndex).Trim();
                            }
                            roleTextBox.Text = roleType;
                        }

                        // Create a PlayerModel instance for the current player
                        var player = new PlayerModel
                        {
                            Id = playerNum,
                            Name = playerName,
                            Role = role,
                            Card = treacheryImagePath,
                            Unveiled = treacheryVisible,
                            Password = password,
                            PlayerCount = playerCount
                        };

                        // Save the player data to a file
                        SavePlayerData(player);

                        string baseFileName = Path.GetFileNameWithoutExtension(role);
                        string ruleFileName = baseFileName + ".txt";
                        string imageFileName = baseFileName + ".jpg";
                        // Get the path to the rules file for Treachery1
                        string rulesFilePath = Path.Combine(Application.StartupPath, "Treachery", ruleFileName);

                        // Assign file names to the static variables based on player number
                        switch (playerNum)
                        {
                            case 1:
                                Form1.TreacheryRule1 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage1 = imageFileName;
                                break;
                            case 2:
                                Form1.TreacheryRule2 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage2 = imageFileName;
                                break;
                            case 3:

                                Form1.TreacheryRule3 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage3 = imageFileName;
                                break;
                            case 4:
                                Form1.TreacheryRule4 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage4 = imageFileName;
                                break;
                            case 5:
                                Form1.TreacheryRule5 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage5 = imageFileName;
                                break;
                            case 6:
                                Form1.TreacheryRule6 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage6 = imageFileName;
                                break;
                            case 7:
                                Form1.TreacheryRule7 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage7 = imageFileName;
                                break;
                            case 8:
                                Form1.TreacheryRule8 = File.ReadAllText(rulesFilePath);
                                Form1.TreacheryImage8 = imageFileName;
                                break;
                        }

                        //Debug.WriteLine($"Treachery1Image: {TreacheryImage1}");
                        //Debug.WriteLine($"TreacheryImage: {imageFileName}");
                        //Debug.WriteLine($"TreacheryRule1: {TreacheryRule1}");
                        

                    }
                }
                else
                {
                    //Console.WriteLine("No available roles.");
                }
            }
            RefreshTreacheryImages();
        }



        // Shuffle method
        private static void Shuffle<T>(List<T> list)
        {
            Random random = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        //Player Name change
        private void Player1_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(1, playerTextBox.Text);
            }
        }
        private void Player2_TextChanged(object sender, EventArgs e)
        {

            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(2, playerTextBox.Text);
            }

        }
        private void Player3_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(3, playerTextBox.Text);
            }
        }
        private void Player4_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(4, playerTextBox.Text);
            }
        }
        private void Player5_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(5, playerTextBox.Text);
            }
        }
        private void Player6_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(6, playerTextBox.Text);
            }
        }
        private void Player7_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(7, playerTextBox.Text);
            }
        }
        private void Player8_TextChanged(object sender, EventArgs e)
        {
            // Assuming Player2's TextBox is named "Player2"
            TextBox playerTextBox = sender as TextBox;
            if (playerTextBox != null)
            {
                // Call the method to update related progress bars
                UpdateRelatedProgressBarNames(8, playerTextBox.Text);
            }
        }




        //Player HP change to display Player dead Label
        private void Player1HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player1HP.Value > 0)
            {
                Player1Dead.Visible = false;
            }
            else
            {
                Player1Dead.Visible = true;
            }
        }
        private void Player2HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player2HP.Value > 0)
            {
                Player2Dead.Visible = false;
            }
            else
            {
                Player2Dead.Visible = true;
            }
        }
        private void Player3HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player3HP.Value > 0)
            {
                Player3Dead.Visible = false;
            }
            else
            {
                Player3Dead.Visible = true;
            }
        }
        private void Player4HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player4HP.Value > 0)
            {
                Player4Dead.Visible = false;
            }
            else
            {
                Player4Dead.Visible = true;
            }
        }
        private void Player5HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player5HP.Value > 0)
            {
                Player5Dead.Visible = false;
            }
            else
            {
                Player5Dead.Visible = true;
            }
        }
        private void Player6HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player6HP.Value > 0)
            {
                Player6Dead.Visible = false;
            }
            else
            {
                Player6Dead.Visible = true;
            }
        }
        private void Player7HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player7HP.Value > 0)
            {
                Player7Dead.Visible = false;
            }
            else
            {
                Player7Dead.Visible = true;
            }
        }
        private void Player8HP_ValueChanged(object sender, EventArgs e)
        {
            // Check if the label is found
            if (Player8HP.Value > 0)
            {
                Player8Dead.Visible = false;
            }
            else
            {
                Player8Dead.Visible = true;
            }
        }


        //Webservice functions
        public string Player1Name
        {
            get { return Player1.Text; }
            set { Player1.Text = value; } // Add this setter
        }// Expose Player1 name
        public string Player2Name { get { return Player2.Text; } } // Expose Player name
        public string Player3Name { get { return Player3.Text; } } // Expose Player name
        public string Player4Name { get { return Player4.Text; } } // Expose Player name
        public string Player5Name { get { return Player5.Text; } } // Expose Player name
        public string Player6Name { get { return Player6.Text; } } // Expose Player name
        public string Player7Name { get { return Player7.Text; } } // Expose Player name
        public string Player8Name { get { return Player8.Text; } } // Expose Player name
        public string Treachery1ImagePath { get { return TreacheryImage1; } } // Expose Treachery1 image path
        public string Treachery2ImagePath { get { return TreacheryImage2; } } // Expose Treachery2 image path
        public string Treachery3ImagePath { get { return TreacheryImage3; } } // Expose Treachery3 image path
        public string Treachery4ImagePath { get { return TreacheryImage4; } } // Expose Treachery4 image path
        public string Treachery5ImagePath { get { return TreacheryImage5; } } // Expose Treachery5 image path
        public string Treachery6ImagePath { get { return TreacheryImage6; } } // Expose Treachery6 image path
        public string Treachery7ImagePath { get { return TreacheryImage7; } } // Expose Treachery7 image path
        public string Treachery8ImagePath { get { return TreacheryImage8; } } // Expose Treachery8 image path

        public string TreacheryRule1Path { get { return TreacheryRule1; } } // Expose Treachery1 image path
        public string TreacheryRule2Path { get { return TreacheryRule2; } } // Expose Treachery1 image path
        public string TreacheryRule3Path { get { return TreacheryRule3; } } // Expose Treachery1 image path
        public string TreacheryRule4Path { get { return TreacheryRule4; } } // Expose Treachery1 image path
        public string TreacheryRule5Path { get { return TreacheryRule5; } } // Expose Treachery1 image path
        public string TreacheryRule6Path { get { return TreacheryRule6; } } // Expose Treachery1 image path
        public string TreacheryRule7Path { get { return TreacheryRule7; } } // Expose Treachery1 image path
        public string TreacheryRule8Path { get { return TreacheryRule8; } } // Expose Treachery1 image path

        public bool Treachery1Visibility { get { return Treachery1.Visible; } }
        public static string passwordP1 { get; set; }

        public string ReturnP1Password
        {
            get { return passwordP1; }
            set { passwordP1 = value; } // Add this setter
        }

        public string ReturnP2Password
        {
            get { return passwordP2; }
            set { passwordP2 = value; } // Add this setter
        }
        public string ReturnP3Password
        {
            get { return passwordP3; }
            set { passwordP3 = value; } // Add this setter
        }

        public string ReturnP4Password
        {
            get { return passwordP4; }
            set { passwordP4 = value; } // Add this setter
        }

        public string ReturnP5Password
        {
            get { return passwordP5; }
            set { passwordP5 = value; } // Add this setter
        }
        public string ReturnP6Password
        {
            get { return passwordP6; }
            set { passwordP6 = value; } // Add this setter
        }

        public string ReturnP7Password
        {
            get { return passwordP7; }
            set { passwordP7 = value; } // Add this setter
        }

        public string ReturnP8Password
        {
            get { return passwordP8; }
            set { passwordP8 = value; } // Add this setter
        }

        public static string passwordP2 { get; set; }
        public static string passwordP3 { get; set; }
        public static string passwordP4 { get; set; }
        public static string passwordP5 { get; set; }
        public static string passwordP6 { get; set; }
        public static string passwordP7 { get; set; }
        public static string passwordP8 { get; set; }

        public int AmountofPlayers { get { return selectedPlayers; } }

        public string Player1Role { get { return RolePlayer1.Text; } }

        // Static fields to hold file names
        private static string treacheryRuleFileName;
        private static string treacheryImageFileName;


        // Properties to expose file names
        public static string TreacheryRule1, TreacheryRule2, TreacheryRule3, TreacheryRule4,
                 TreacheryRule5, TreacheryRule6, TreacheryRule7, TreacheryRule8;

        public static string TreacheryImage1, TreacheryImage2, TreacheryImage3, TreacheryImage4,
                             TreacheryImage5, TreacheryImage6, TreacheryImage7, TreacheryImage8;

        private async void Commander1_TextChanged(object sender, EventArgs e)
        {
            // Assuming CommanderSelector is your ComboBox name
            var selectedCommander = CommanderSelector.SelectedItem.ToString();

            // Assuming the naming convention is "CommanderImageX" where X is the player number
            PictureBox correspondingPictureBox = this.Controls.Find(selectedCommander, true).FirstOrDefault() as PictureBox;

            // Fetch the name from the corresponding TextBox. Since there's only one TextBox named "Commander1", we can hardcode it.
            TextBox correspondingTextBox = this.Controls.Find("Commander1", true).FirstOrDefault() as TextBox;

            if (correspondingTextBox != null && correspondingPictureBox != null)
            {
                string cardName = correspondingTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(cardName))
                {
                    try
                    {
                        await FetchAndDisplayCardImage(cardName, correspondingPictureBox); // Call the new method
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a card name.");
                }
            }
        }

        private void wificonnector_Click(object sender, EventArgs e)
        {
            // Check the current text of the button to determine the action
            if (wificonnector.Text == "Show Wifi")
            {
                // Change the button text to "Treachery Page"
                wificonnector.Text = "Treachery Page";

                // Set visibility: Hide QRCode, Show WifiConnect
                QRCode.Visible = false;
                WifiConnect.Visible = true;
            }
            else if (wificonnector.Text == "Treachery Page")
            {
                // Change the button text to "Wifi Connect"
                wificonnector.Text = "Show Wifi";

                // Set visibility: Show QRCode, Hide WifiConnect
                QRCode.Visible = true;
                WifiConnect.Visible = false;
            }
        }


        private void ResetPassword_Click(object sender, EventArgs e)
        {
            ResetPasswords();
        }

        private void ResetPasswords()
        {
            passwordP1 = "1234";
            passwordP2 = "1234";
            passwordP3 = "1234";
            passwordP4 = "1234";
            passwordP5 = "1234";
            passwordP6 = "1234";
            passwordP7 = "1234";
            passwordP8 = "1234";

        }

        private void CommanderSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if any item is selected in CommanderSelector
            if (CommanderSelector.SelectedItem != null)
            {
                // Get the selected card name from the combobox
                string selectedCardName = CommandComboBox1.SelectedItem as string;

                // Get the corresponding PictureBox based on the selected item in CommanderSelector
                string pictureBoxName = CommanderSelector.SelectedItem.ToString().Replace("CommanderImage", "CommanderImage");
                PictureBox selectedPictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;

                // Update the corresponding PictureBox with the selected card image
                if (selectedPictureBox != null && !string.IsNullOrWhiteSpace(selectedCardName))
                {
                    _ = FetchAndDisplayCardImage(selectedCardName, selectedPictureBox); // Suppress the warning
                }
            }
        }




        // Method to save the player data to a file
        private void SavePlayerData(PlayerModel player)
        {
            string jsonData = JsonConvert.SerializeObject(player);
            File.WriteAllText($"PlayerData_Player{player.Id}.json", jsonData);
        }

        private void SavePlayerCount(int playerCount)
        {
            File.WriteAllText("PlayerCountConfig.json", JsonConvert.SerializeObject(playerCount));
        }

        private void P5CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void P7CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void P6CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void P8CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void P4CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void Player1PoisonDMG_Click(object sender, EventArgs e)
        {

        }

        private void P3CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void P2CommanderDMGP1_Click(object sender, EventArgs e)
        {

        }

        private void Player4PoisonDMG_Click(object sender, EventArgs e)
        {

        }
    }
}
