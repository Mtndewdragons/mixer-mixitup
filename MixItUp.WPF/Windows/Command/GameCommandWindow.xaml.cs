﻿using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window.Command;
using MixItUp.WPF.Controls.Games;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for GameCommandWindow.xaml
    /// </summary>
    public partial class GameCommandWindow : LoadingWindowBase
    {
        private GameCommandWindowViewModel viewModel;

        private Dictionary<string, GameEditorControlBase> gameEditors = new Dictionary<string, GameEditorControlBase>();
        private GameEditorControlBase gameEditor;

        public GameCommandWindow(GameCommandBase command)
            : this()
        {
            this.viewModel = new GameCommandWindowViewModel(command);
        }

        public GameCommandWindow()
        {
            InitializeComponent();

            this.viewModel = new GameCommandWindowViewModel();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            this.viewModel.GameTypeSelected += ViewModel_GameTypeSelected;
            await this.viewModel.OnLoaded();

            if (this.viewModel.GameCommand != null)
            {
                if (this.viewModel.GameCommand is SpinGameCommand) { this.SetGameEditorControl(new SpinGameEditorControl((SpinGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is SlotMachineGameCommand) { this.SetGameEditorControl(new SlotMachineGameEditorControl((SlotMachineGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is VendingMachineGameCommand) { this.SetGameEditorControl(new VendingMachineGameEditorControl((VendingMachineGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is StealGameCommand) { this.SetGameEditorControl(new StealGameEditorControl((StealGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is PickpocketGameCommand) { this.SetGameEditorControl(new PickpocketGameEditorControl((PickpocketGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is DuelGameCommand) { this.SetGameEditorControl(new DuelGameEditorControl((DuelGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is HeistGameCommand) { this.SetGameEditorControl(new HeistGameEditorControl((HeistGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is RussianRouletteGameCommand) { this.SetGameEditorControl(new RussianRouletteGameEditorControl((RussianRouletteGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is BidGameCommand) { this.SetGameEditorControl(new BidGameEditorControl((BidGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is RouletteGameCommand) { this.SetGameEditorControl(new RouletteGameEditorControl((RouletteGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is HitmanGameCommand) { this.SetGameEditorControl(new HitmanGameEditorControl((HitmanGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is CoinPusherGameCommand) { this.SetGameEditorControl(new CoinPusherGameEditorControl((CoinPusherGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is VolcanoGameCommand) { this.SetGameEditorControl(new VolcanoGameEditorControl((VolcanoGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is LockBoxGameCommand) { this.SetGameEditorControl(new LockBoxGameEditorControl((LockBoxGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is WordScrambleGameCommand) { this.SetGameEditorControl(new WordScrambleGameEditorControl((WordScrambleGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is BetGameCommand) { this.SetGameEditorControl(new BetGameEditorControl((BetGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is HangmanGameCommand) { this.SetGameEditorControl(new HangmanGameEditorControl((HangmanGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is TreasureDefenseGameCommand) { this.SetGameEditorControl(new TreasureDefenseGameEditorControl((TreasureDefenseGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is HotPotatoGameCommand) { this.SetGameEditorControl(new HotPotatoGameEditorControl((HotPotatoGameCommand)this.viewModel.GameCommand)); }
                else if (this.viewModel.GameCommand is BeachBallGameCommand) { this.SetGameEditorControl(new BeachBallGameEditorControl((BeachBallGameCommand)this.viewModel.GameCommand)); }
            }
            else
            {
                this.gameEditors.Add("Spin", new SpinGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Slot Machine",  new SlotMachineGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Vending Machine", new VendingMachineGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Steal", new StealGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Pickpocket", new PickpocketGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Duel", new DuelGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Heist", new HeistGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Russian Roulette", new RussianRouletteGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Roulette", new RouletteGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Bid", new BidGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Hitman", new HitmanGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Coin Pusher", new CoinPusherGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Volcano", new VolcanoGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Lock Box", new LockBoxGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Word Scramble", new WordScrambleGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Bet", new BetGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Hangman", new HangmanGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Treasure Defense", new TreasureDefenseGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Hot Potato", new HotPotatoGameEditorControl(this.viewModel.DefaultCurrency));
                this.gameEditors.Add("Beach Ball", new BeachBallGameEditorControl(this.viewModel.DefaultCurrency));
            }
        }

        private void ViewModel_GameTypeSelected(object sender, GameTypeListing e)
        {
            this.SetGameEditorControl(this.gameEditors[this.viewModel.SelectedGameType.Name]);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (await gameEditor.Validate())
                {
                    gameEditor.SaveGameCommand();
                    this.Close();
                }
            });
        }

        private void SetGameEditorControl(GameEditorControlBase gameEditor)
        {
            this.MainContentControl.Content = this.gameEditor = gameEditor;
        }
    }
}
