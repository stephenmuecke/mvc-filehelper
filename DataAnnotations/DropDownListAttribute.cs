using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Sandtrap.Web.DataAnnotations
{

    /// <summary>
    /// Defines an attribute to generate a html select element for the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DropDownListAttribute : Attribute, IMetadataAware
    {

        #region .Declarations 

        private const string _PropertyNotFound = "A property with the name {0} does not exist in the class";
        private const string _InvalidSelectList = "The property {0} is does not implement IEnumerable<SelectListItem>";

        #endregion

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with default properties.
        /// </summary>
        public DropDownListAttribute()
        {
            // Set defaults
            OptionLabel = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with default properties.
        /// </summary>
        public DropDownListAttribute(string selectListProperty)
        {
            SelectListProperty = selectListProperty;
            // Set defaults
            OptionLabel = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of DropDownListAttribute with the specified properties.
        /// </summary>
        /// <param name="selectListProperty">
        /// The name of the property used to generate the option elements.
        /// </param>
        /// <param name="optionLabel">
        /// The text for a default empty option.
        /// </param>
        public DropDownListAttribute(string selectListProperty, string optionLabel)
        {
            SelectListProperty = selectListProperty;
            OptionLabel = optionLabel;
        }

        #endregion

        #region .Metadata keys 

        /// <summary>
        /// Gets the key for the metadata SelectListProperty property
        /// </summary>
        public static string SelectListPropertyKey
        {
            get { return "SelectListProperty"; }
        }

        /// <summary>
        /// Gets the key for the metadata OptionLabel property
        /// </summary>
        public static string OptionLabelKey
        {
            get { return "OptionLabel"; }
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the name of the property used to generate the option elements.
        /// </summary>
        public string SelectListProperty { get; set; }

        /// <summary>
        /// Gets or sets the text for a default empty option.
        /// </summary>
        public string OptionLabel { get; set; }

        #endregion

        #region .Methods 

        /// <summary>
        /// Adds additional metadata values used to render the select element.
        /// </summary>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            // Add metadata
            metadata.AdditionalValues[SelectListPropertyKey] = SelectListProperty;
            metadata.AdditionalValues[OptionLabelKey] = OptionLabel;
        }

        #endregion


    }


}
