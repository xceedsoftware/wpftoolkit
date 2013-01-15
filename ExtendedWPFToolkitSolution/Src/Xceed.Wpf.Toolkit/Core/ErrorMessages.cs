/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Resources;

namespace Xceed.Wpf.Toolkit.Core
{
  internal static class ErrorMessages
  {
    #region Static Fields


    public const string EndAngleCannotBeSetDirectlyInSlice = "EndAngleCannotBeSetDirectlyInSlice";
    public const string SliceCannotBeSetDirectlyInEndAngle = "SliceCannotBeSetDirectlyInEndAngle";
    public const string SliceOOR = "SliceOOR";
    public const string AnimationAccelerationRatioOOR = "AnimationAccelerationRatioOOR";
    public const string AnimationDecelerationRatioOOR = "AnimationDecelerationRatioOOR";
    public const string ZoomboxContentMustBeUIElement = "ZoomboxContentMustBeUIElement";
    public const string ViewModeInvalidForSource = "ViewModeInvalidForSource";
    public const string ZoomboxTemplateNeedsContent = "ZoomboxTemplateNeedsContent";
    public const string ZoomboxHasViewFinderButNotDisplay = "ZoomboxHasViewFinderButNotDisplay";
    public const string PositionOnlyAccessibleOnAbsolute = "PositionOnlyAccessibleOnAbsolute";
    public const string ZoomboxViewAlreadyInitialized = "ZoomboxViewAlreadyInitialized";
    public const string ScaleOnlyAccessibleOnAbsolute = "ScaleOnlyAccessibleOnAbsolute";
    public const string RegionOnlyAccessibleOnRegionalView = "RegionOnlyAccessibleOnRegionalView";
    public const string UnableToConvertToZoomboxView = "UnableToConvertToZoomboxView";
    public const string ViewStackCannotBeManipulatedNow = "ViewStackCannotBeManipulatedNow";

    public const string SuppliedValueWasNotVisibility = "SuppliedValueWasNotVisibility";

    private static readonly ResourceManager _resourceManager;

    #endregion

    #region Constructor

    static ErrorMessages()
    {
      _resourceManager = new ResourceManager( "Xceed.Wpf.Toolkit.Core.ErrorMessages", typeof( ErrorMessages ).Assembly );
    }

    #endregion

    public static string GetMessage( string msgId )
    {
      return _resourceManager.GetString( msgId );
    }
  }
}
