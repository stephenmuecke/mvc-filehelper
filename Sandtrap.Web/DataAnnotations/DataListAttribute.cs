using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Sandtrap.Web.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Property)]
    public class DataListAttribute : Attribute, IMetadataAware
    {

        #region .Declarations 

        private const string _PropertyNotFound = "A property with the name {0} does not exist in the class";
        private const string _InvalidDataList = "The property {0} is does not implement IEnumerable<string>";

        #endregion

        #region .Constructors 

        /// <summary>
        /// Initialises a new instance of DataListAttribute with default properties.
        /// </summary>
        public DataListAttribute()
        {
        }

        /// <summary>
        /// Initialises a new instance of DataListAttribute with specified properties.
        /// </summary>
        /// <param name="dataListProperty">
        /// The name of the property used to generate the option elements.
        /// </param>
        public DataListAttribute(string dataListProperty)
        {
            DataListProperty = dataListProperty;
        }

        #endregion

        #region .Metadata keys 

        /// <summary>
        /// Gets the key for the metadata DataListProperty property
        /// </summary>
        public static string DataListPropertyKey
        {
            get { return "DataListProperty"; }
        }

        #endregion

        #region .Properties 

        /// <summary>
        /// Gets or sets the name of the property used to generate the option elements.
        /// </summary>
        public string DataListProperty { get; set; }

        #endregion

        #region .Methods 

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            // Add metadata
            metadata.AdditionalValues[DataListPropertyKey] = DataListProperty;
        }

        #endregion

    }
}
