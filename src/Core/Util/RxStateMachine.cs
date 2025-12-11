using Stateless;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;

namespace ModManager.Util;

public class RxStateMachine<TState, TTrigger>
{
	private readonly StateMachine<TState, TTrigger> _stateMachine;
	private readonly Subject<TTrigger> _subject;

	private readonly ReactiveCommand<TTrigger, Unit> _handleTriggerCommand;

	public RxStateMachine(TState initialState, IScheduler? fireScheduler = null)
	{
		_subject = new();
		_stateMachine = new StateMachine<TState, TTrigger>(initialState);

		fireScheduler ??= RxApp.TaskpoolScheduler;

		_handleTriggerCommand = ReactiveCommand.CreateFromTask<TTrigger>(HandleTriggerAsync);
		//_subject.ObserveOn(fireScheduler).Do(x => HandleTriggerAsync(x).ToObservable()).Subscribe();
		_subject.ObserveOn(fireScheduler).InvokeCommand(_handleTriggerCommand);
	}

	public StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state) => _stateMachine.Configure(state);

	public void Enqueue(TTrigger trigger) => _subject.OnNext(trigger);
	private async Task HandleTriggerAsync(TTrigger trigger) => await _stateMachine.FireAsync(trigger);
}
