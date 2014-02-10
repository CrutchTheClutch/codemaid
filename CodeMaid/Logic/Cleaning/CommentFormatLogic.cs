﻿#region CodeMaid is Copyright 2007-2013 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify it under the terms of the GNU
// Lesser General Public License version 3 as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2013 Steve Cadwallader.

using System;
using System.Linq;
using EnvDTE;
using SteveCadwallader.CodeMaid.Helpers;
using SteveCadwallader.CodeMaid.Properties;

namespace SteveCadwallader.CodeMaid.Logic.Cleaning
{
    /// <summary>
    /// A class for encapsulating comment formatting logic.
    /// </summary>
    internal class CommentFormatLogic
    {
        #region Fields

        private readonly CachedSettingSet<string> _majorTags =
            new CachedSettingSet<string>(() => Settings.Default.Cleaning_CommentMajorTags,
                                         expression =>
                                         expression.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim().ToLower())
                                                   .Where(y => !string.IsNullOrEmpty(y))
                                                   .ToList());

        private readonly CachedSettingSet<string> _minorTags =
            new CachedSettingSet<string>(() => Settings.Default.Cleaning_CommentMinorTags,
                                         expression =>
                                         expression.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim().ToLower())
                                                   .Where(y => !string.IsNullOrEmpty(y))
                                                   .ToList());

        private readonly CodeMaidPackage _package;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        private static CommentFormatLogic _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CommentFormatLogic(CodeMaidPackage package)
        {
            _package = package;
        }

        /// <summary>
        /// Gets an instance of the <see cref="CommentFormatLogic" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CommentFormatLogic" /> class.</returns>
        internal static CommentFormatLogic GetInstance(CodeMaidPackage package)
        {
            return _instance ?? (_instance = new CommentFormatLogic(package));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Reformat all comments in the specified document.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        public void FormatComments(TextDocument textDocument)
        {
            if (!Settings.Default.Cleaning_CommentRunDuringCleanup) return;

            FormatComments(textDocument, textDocument.StartPoint.CreateEditPoint(), textDocument.EndPoint.CreateEditPoint());
        }

        /// <summary>
        /// Reformat all comments between the specified start and end point. Comments that start
        /// within the range, even if they overlap the end are included.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        public bool FormatComments(TextDocument document, EditPoint start, EditPoint end)
        {
            int maxWidth = Math.Max(Settings.Default.Cleaning_CommentWrapColumn, 20);

            bool foundComments = false;

            while (start.Line <= end.Line)
            {
                if (CodeCommentHelper.IsCommentLine(start))
                {
                    var comment = new CodeComment(start, this._package);
                    if (comment.IsValid)
                    {
                        comment.Format(maxWidth);
                        foundComments = true;
                    }

                    start = comment.EndPoint.CreateEditPoint();
                    start.LineDown();
                    start.StartOfLine();
                }
                else
                {
                    if (start.Line == document.EndPoint.Line)
                        break;

                    start.LineDown();
                }
            }

            return foundComments;
        }

        #endregion Methods
    }
}