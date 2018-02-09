using System;
using UniRx;

namespace M1.Utilities.Rx
{
    /// <summary>
    /// LinkedObservable will run chain of observable streams one after another
    /// </summary>
    public class LinkedObservable : IDisposable
    {
        /// <summary>
        /// _queHistory, where holds all qued streams
        /// </summary>
        private CompositeDisposable _queHistory = new CompositeDisposable();
        private readonly object _gate = new object();
        /// <summary>
        /// _tail, last stream
        /// </summary>
        private AsyncSubject<Unit> _tail;
        
        /// <summary>
        /// Adding new source stream to run
        /// </summary>
        /// <param name="Source">any observable streams</param>
        public void Add<T>(UniRx.IObservable<T> Source)
        {
            lock (_gate)
            {
                if (_tail != null)
                {
                    var newHead = new AsyncSubject<Unit>();
                
                    _tail
                        .Subscribe(_ =>
                        {
                            var streamToLink = 
                                Source
                                    .DoOnCompleted(() =>
                                    {
                                        newHead.OnNext(Unit.Default);
                                        newHead.OnCompleted();
                                    }).Subscribe();
        
                            _queHistory.Add(streamToLink);
                        });
                    _tail = newHead;
                    _queHistory.Add(_tail);
                }
                else
                {
                    var  head = new AsyncSubject<Unit>();
               
                    var initialStream = 
                        Source
                            .DoOnCompleted(() =>
                            {
                                head.OnNext(Unit.Default);
                                head.OnCompleted();
                            }).Subscribe();
        
                    _queHistory.Add(initialStream);
                    _tail = head;
                    _queHistory.Add(_tail);
                }
            }
        }

        /// <summary>
        /// Adding pair of sources stream to run
        /// </summary>
        /// <param name="Source">any observable streams</param>
        public void AddPair<T>(UniRx.IObservable<T> Source1,UniRx.IObservable<T> Source2)
        {
            lock (_gate)
            {
                if (_tail != null)
                {
                    var newHead = new AsyncSubject<Unit>();
                    _tail
                        .Subscribe(_ =>
                        {
                            var streamToLink = 
                                Observable.WhenAll(Source1, Source2)
                                    .DoOnCompleted(() =>
                                    {
                                        newHead.OnNext(Unit.Default);
                                        newHead.OnCompleted();
                                    }).Subscribe();
        
                            _queHistory.Add(streamToLink);
                        });
                    _tail = newHead;
                    _queHistory.Add(_tail);
                }
                else
                {
                    var  head = new AsyncSubject<Unit>();
                    var initialStream =

                        Observable.WhenAll(Source1, Source2)
                            .DoOnCompleted(() =>
                            {
                                head.OnNext(Unit.Default);
                                head.OnCompleted();
                            }).Subscribe();
                        
                    _queHistory.Add(initialStream);
                    _tail = head;
                    _queHistory.Add(_tail);
                }
            }
        }
        
        /// <summary>
        /// Cancle all runnining streams, clearing _queHistory
        /// </summary>
        public void Cancle()
        {
            _queHistory.Clear();
            _tail = null;
        }
        /// <summary>
        /// this doens't do anything
        /// </summary>
        public IDisposable Subscribe()
        {
            return Disposable.Empty;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            if (_queHistory != null) _queHistory.Dispose();
        }
  
    }
}


