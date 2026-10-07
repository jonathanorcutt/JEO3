namespace JEO3.Generation.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FormattingSettings
    {
        #region Properties

        /// <summary>
        /// Bitwise flags to turn specific code comments, metrics, or ASCII decorations on/off.
        /// </summary>
        public AnnotationVerbosity AdvancedAnnotations { get; set; } =
            AnnotationVerbosity.DoNotRemoveAscii |
            AnnotationVerbosity.ExecutionSummary |
            AnnotationVerbosity.PathBreadcrumbs |
            AnnotationVerbosity.TableFooter |
            AnnotationVerbosity.TraversalSummary |
            AnnotationVerbosity.DoNotRemoveAscii;

        /// <summary>
        /// Breaks table projections onto individual code lines rather than packing them horizontally.
        /// </summary>
        public bool SingleLinePerTable { get; init; } = true;

        /// <summary>
        /// The number of spaces to indent each level of the traversal path in the generated SQL comments (e.g. "dbo.TableA -> dbo.TableB" would have an indentation applied to "dbo.TableB" based on its depth relative to the root).
        /// </summary>
        public int? BreadcrumbPaddingRight { get; init; }

        #endregion

        #region Initialization

        public FormattingSettings(AnnotationVerbosity advancedAnnotations, bool singleLinePerTable, int? breadcrumbPaddingRight)
        {
            AdvancedAnnotations = advancedAnnotations;
            SingleLinePerTable = singleLinePerTable;
            BreadcrumbPaddingRight = breadcrumbPaddingRight ?? 50;
        }

        #endregion
    }
}