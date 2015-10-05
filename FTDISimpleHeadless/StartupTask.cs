// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using FTDISimple;

namespace FTDISimpleHeadless
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;


        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            FTDISimple.FTDISimple.Init();     
        }

    }
}
