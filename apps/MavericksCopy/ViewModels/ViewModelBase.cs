using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Serilog;

namespace MavericksCopy.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value,
            [CallerMemberName] string? prop = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(prop);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        // ── Shared UI state ───────────────────────────────────────
        private bool    _isBusy;
        private string? _statusMessage;

        public bool IsNotBusy => !_isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            protected set { SetField(ref _isBusy, value); OnPropertyChanged(nameof(IsNotBusy)); }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            protected set => SetField(ref _statusMessage, value);
        }

        // ── Guarded async runner ─────────────────────────────────
        protected async Task RunAsync(Func<Task> work,
            [CallerMemberName] string? caller = null)
        {
            IsBusy        = true;
            StatusMessage = null;
            try   { await work(); }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                Log.Error(ex, "[{VM}.{Caller}] Unhandled", GetType().Name, caller);
            }
            finally { IsBusy = false; }
        }
    }

    // ── RelayCommand ────────────────────────────────────────────────
    public sealed class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object?>       _exec;
        private readonly Func<object?, bool>?  _can;

        public RelayCommand(Action exec, Func<bool>? can = null)
            : this(_ => exec(), can is null ? null : _ => can()) { }

        public RelayCommand(Action<object?> exec, Func<object?, bool>? can = null)
        { _exec = exec; _can = can; }

        public event EventHandler? CanExecuteChanged
        {
            add    => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }
        public bool CanExecute(object? p) => _can?.Invoke(p) ?? true;
        public void Execute(object? p)    => _exec(p);
        public void Refresh()
            => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    // ── AsyncRelayCommand ──────────────────────────────────────────
    public sealed class AsyncRelayCommand : System.Windows.Input.ICommand
    {
        private readonly Func<Task>  _exec;
        private readonly Func<bool>? _can;
        private bool _running;

        public AsyncRelayCommand(Func<Task> exec, Func<bool>? can = null)
        { _exec = exec; _can = can; }

        public event EventHandler? CanExecuteChanged
        {
            add    => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }
        public bool CanExecute(object? _) => !_running && (_can?.Invoke() ?? true);
        public async void Execute(object? _)
        {
            _running = true;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            try   { await _exec(); }
            finally
            {
                _running = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
