﻿using Lively.Common;
using Lively.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lively.UI.WinUI.Factories
{
    public class ApplicationsRulesFactory : IApplicationsRulesFactory
    {
        public ApplicationRulesModel CreateAppRule(string appPath, AppRulesEnum rule)
        {
            var fileName = Path.GetFileNameWithoutExtension(appPath);
            return new ApplicationRulesModel(fileName, rule);
        }
    }
}
