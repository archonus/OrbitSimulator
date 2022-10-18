using System.Threading.Tasks;

namespace OrbitSimulator
{
    /// <summary>
    /// A class for creating databindings on the result of asynchronus operations
    /// </summary>
    /// <typeparam name="T">The type of the result of the task</typeparam>
    public class BindableTask<T> : BindingSourceBase
    {
        readonly T defaultResult = default(T); //The default result is the default value of the type T

        /// <summary>
        /// The result of the task, accessed in a non-blocking way
        /// </summary>
        /// <remarks>Default value if the task is not completed or failed to complete</remarks>
        public T Result
        {
            get
            { //Return the default value if the task is not completed or failed to complete, otherwise the result
                return IsSuccessful ? ResultTask.Result : defaultResult;
            }
        }

        /// <summary>
        /// The task that will get the result
        /// </summary>
        public Task<T> ResultTask { get; private set; }

        /// <summary>
        /// Whether the task is completed AND was successful (not cancelled or failed)
        /// </summary>
        public bool IsSuccessful => ResultTask.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// The status of the task
        /// </summary>
        public TaskStatus Status { get => ResultTask.Status; }

        #region Constructors
        /// <summary>
        /// Constructs a <see cref="BindableTask{T}"/>, with default value as the default value of <see cref="T"/>
        /// </summary>
        /// <param name="task">The task whose result is being bound to</param>
        public BindableTask(Task<T> task)
        {
            ResultTask = task;
            if (!ResultTask.IsCompleted)
            {
                _ = WatchTask(task);
            }
        }

        /// <summary>
        /// Constructs a <see cref="BindableTask{T}"/> with the specified default value
        /// </summary>
        /// <param name="task">The task whose result is being bound to</param>
        /// <param name="defaultResult">The result that will be returned while the task has completed</param>
        public BindableTask(Task<T> task, T defaultResult) : this(task)
        {
            this.defaultResult = defaultResult;
        }
        #endregion

        protected async Task WatchTask(Task<T> task)
        {
            try
            {
                await task;
            }
            catch { }
            OnPropertyChanged(nameof(Result));
            OnPropertyChanged(nameof(IsSuccessful));
            OnPropertyChanged(nameof(Status));
        }
    }
}
