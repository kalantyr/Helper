﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Helper.Checkers;

namespace Helper.Jobs
{
    public interface IJob
    {
        string Name { get; }

        ICheckerHistory History { get; }

        Action<IJob, string> Message { get; set; }

        Task Run(CancellationToken cancellationToken);
    }
}