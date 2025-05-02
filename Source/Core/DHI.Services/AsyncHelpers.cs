namespace DHI.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Helper methods for running async methods synchronously.
    /// </summary>
    /// <remarks>
    ///     Copied from https://stackoverflow.com/a/25097498/798781
    /// </remarks>
    public static class AsyncHelpers
    {
        private static readonly TaskFactory _taskFactory = new(CancellationToken.None,
                                                               TaskCreationOptions.None,
                                                               TaskContinuationOptions.None,
                                                               TaskScheduler.Default);

        /// <summary>
        ///     Run any Task synchronously.
        /// </summary>
        /// <remarks>
        ///     It is possible to call <see cref="Task.RunSynchronously()"/> if the Task is the 'Created' state. However, if the Task is returned from a
        ///     function call where it cannot be validated to be in the 'Created' state calling <see cref="Task.RunSynchronously()"/> would result in a runtime error.
        ///     <br/>
        ///     <br/>
        ///     Calling this method is guaranteed to run on any Task
        /// </remarks>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _taskFactory
                   .StartNew(func)
                   .Unwrap()
                   .GetAwaiter()
                   .GetResult();
        }

        /// <summary>
        ///     Run any Task synchronously.
        /// </summary>
        /// <remarks>
        ///     It is possible to call <see cref="Task.RunSynchronously()"/> if the Task is the 'Created' state. However, if the Task is returned from a
        ///     function call where it cannot be validated to be in the 'Created' state calling <see cref="Task.RunSynchronously()"/> would result in a runtime error.
        ///     <br/>
        ///     <br/>
        ///     Calling this method is guaranteed to run on any Task
        /// </remarks>
        public static void RunSync(Func<Task> func)
        {
            _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}