using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.ComponentModel;
using System.Reflection;

namespace CesiumForUnity
{
    /// <summary>
    /// Reports the status of a CesiumPropertyTableProperty. If the property
    /// table property cannot be accessed, this briefly indicates why.
    /// </summary>
    public enum CesiumPropertyTablePropertyStatus
    {
        /// <summary>
        /// The property table property is valid.
        /// </summary>
        Valid = 0,
        /// <summary>
        /// The property table property is empty but has a specified default value.
        /// </summary>
        EmptyPropertyWithDefault,
        /// <summary>
        /// The property table property does not exist in the glTF, or the property
        /// definition itself contains errors.
        /// </summary>
        ErrorInvalidProperty,
        /// <summary>
        /// The data associated with the property table property is malformed and
        /// cannot be retrieved.
        /// </summary>
        ErrorInvalidPropertyData
    }

    /// <summary>
    /// Represents a glTF property table property in EXT_structural_metadata.
    /// A property has a specific type, such as int64 scalar or string, and 
    /// values of that type that can be accessed with primitive feature IDs 
    /// from EXT_mesh_features.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumPropertyTablePropertyImpl", "CesiumPropertyTablePropertyImpl.h")]
    public partial class CesiumPropertyTableProperty
    {
        #region Getters
        /// <summary>
        /// The status of the property table property. If this property table 
        /// property is invalid in any way, this will briefly indicate why.
        /// </summary>
        public CesiumPropertyTablePropertyStatus status
        {
            get; internal set;
        }

        /// <summary>
        /// The type of the metadata value as defined in the 
        /// EXT_structural_metadata extension. Some of these types are not 
        /// accessible from Unity, but can be converted to a close-fitting type.
        /// </summary>
        public CesiumMetadataValueType valueType
        {
            get; internal set;
        }

        /// <summary>
        /// The number of values in the property.
        /// </summary>
        public Int64 size
        {
            get; internal set;
        }

        /// <summary>
        /// The number of elements in an array of this property. Only 
        /// applicable when the property is a fixed-length array type.
        /// </summary>
        public Int64 arraySize
        {
            get; internal set;
        }

        /// <summary>
        /// Whether this property is normalized. Only applicable when this 
        /// property has an integer component type.
        /// </summary>
        public bool isNormalized
        {
            get; internal set;
        }

        /// <summary>
        /// The offset of this property. This can be defined by the class property 
        /// that it implements, or overridden by the instance of the property itself.
        /// </summary>
        /// <remarks>
        /// This is only applicable to properties with floating-point or normalized
        /// integer component types. If an offset is not defined or applicable, this
        /// returns an empty value.
        /// </remarks>
        public CesiumMetadataValue offset
        {
            get; internal set;
        }

        /// <summary>
        /// The scale of this property. This can be defined by the class property 
        /// that it implements, or overridden by the instance of the property itself.
        /// </summary>
        /// <remarks>
        /// This is only applicable to properties with floating-point or normalized
        /// integer component types. If a scale is not defined or applicable, this
        /// returns an empty value.
        /// </remarks>
        public CesiumMetadataValue scale
        {
            get; internal set;
        }

        /// <summary>
        /// The minimum value of this property. This can be defined by the class 
        /// property that it implements, or overridden by the instance of the property
        /// itself.
        /// </summary>
        /// <remarks>
        /// This is only applicable to scalar, vecN and matN properties. It represents
        /// the component-wise minimum of all property values with normalization,
        /// offset, and scale applied. If a minimum value is not defined or applicable,
        /// this returns an empty value.
        /// </remarks>
        public CesiumMetadataValue min
        {
            get; internal set;
        }

        /// <summary>
        /// The maximum value of this property. This can be defined by the class 
        /// property that it implements, or overridden by the instance of the property
        /// itself.
        /// </summary>
        /// <remarks>
        /// This is only applicable to scalar, vecN and matN properties. It represents
        /// the component-wise maximum of all property values with normalization,
        /// offset, and scale applied. If a maximum value is not defined or applicable,
        /// this returns an empty value.
        /// </remarks>
        public CesiumMetadataValue max
        {
            get; internal set;
        }

        /// <summary>
        /// The "no data" value of this property, as defined by its class 
        /// property. This value functions a sentinel value, indicating missing
        /// data wherever it appears. The value is compared against the property's
        /// raw data, without normalization, offset, or scale applied. 
        /// </summary>
        /// <remarks>
        /// This is not applicable to boolean properties. If a "no data" value is
        /// not defined or applicable, this returns an empty value.
        /// </remarks>
        public CesiumMetadataValue noData
        {
            get; internal set;
        }

        /// <summary>
        /// The default value of this property, as defined by its class property.
        /// This default value is used when encountering a "no data" value 
        /// in the property.
        /// </summary>
        /// <remarks>
        /// If a default value is not defined, this returns an empty value.
        /// </remarks>
        public CesiumMetadataValue defaultValue
        {
            get; internal set;
        }
        #endregion

        internal CesiumPropertyTableProperty()
        {
            this.status = CesiumPropertyTablePropertyStatus.ErrorInvalidProperty;
            this.valueType = new CesiumMetadataValueType(CesiumMetadataType.Invalid, CesiumMetadataComponentType.None, false);
            this.size = 0;
            this.arraySize = 0;
            this.isNormalized = false;
            this.CreateImplementation();
        }

        #region Public Methods
        /// <summary>
        /// Attempts to retrieve the value for the given feature as a boolean.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset 
        /// before it is further converted. If the raw value is equal to the 
        /// property's "no data" value, then the property's default value will
        /// be converted if possible. If the property-defined default value 
        /// cannot be converted, or does not exist, then the user-defined 
        /// default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is a boolean, it is returned as-is.<br/>
        /// 
        /// - If the value is a scalar, zero is converted to false, while any 
        /// other value is converted to true.<br/>
        /// 
        /// - If the value is a string, "0", "false", and "no" (case-insensitive)
        /// are converted to false, while "1", "true", and "yes" are converted to
        /// true. All other strings, including strings that can be converted to 
        /// numbers, will return the user-defined default value.<br/><br/>
        /// </para>
        /// <para>
        /// All other types return the user-defined default value. If the feature ID is
        /// out-of-range, or if the property table property is somehow invalid, the
        /// user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Boolean.</returns>
        public partial Boolean GetBoolean(Int64 featureID, Boolean defaultValue = false);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 8-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between -128 and 127, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -128 and 127, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a SByte.</returns>
        public partial SByte GetSByte(Int64 featureID, SByte defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 8-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between 0 and 255, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and 255, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Byte.</returns>
        public partial Byte GetByte(Int64 featureID, Byte defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 16-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between -32768 and 32767, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -32768 and 32767, the parsed value is returned. The string 
        /// is parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Int16.</returns>
        public partial Int16 GetInt16(Int64 featureID, Int16 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 16-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between 0 and 65535, it is returned as-is.
        /// Otherwise, if the value is a floating-point number in the aforementioned
        /// range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and 65535, the parsed value is returned. The string is
        /// parsed in a locale-independent way and does not support the use of commas
        /// or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a UInt16.</returns>
        public partial UInt16 GetUInt16(Int64 featureID, UInt16 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 32-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between -2,147,483,648 and 2,147,483,647,
        /// it is returned as-is. Otherwise, if the value is a floating-point number 
        /// in the aforementioned range, it is truncated (rounded toward zero) and 
        /// returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -2,147,483,648 and 2,147,483,647, the parsed value is 
        /// returned. The string is parsed in a locale-independent way and does not
        /// support the use of commas or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Int32</returns>
        public partial Int32 GetInt32(Int64 featureID, Int32 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 32-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between 0 and 4,294,967,295, it is returned 
        /// as-is. Otherwise, if the value is a floating-point number in the 
        /// aforementioned range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and 4,294,967,295, the parsed value is returned. The 
        /// string is parsed in a locale-independent way and does not support the use 
        /// of commas or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a UInt32.</returns>
        public partial UInt32 GetUInt32(Int64 featureID, UInt32 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a signed
        /// 64-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between -2^63 and (2^63 - 1), it is returned 
        /// as-is. Otherwise, if the value is a floating-point number in the 
        /// aforementioned  range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between -2^63 and (2^63 - 1), the parsed value is returned. The string
        /// is parsed in a locale-independent way and does not support the use of commas 
        /// or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Int64</returns>
        public partial Int64 GetInt64(Int64 featureID, Int64 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as an unsigned
        /// 64-bit integer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is an integer between 0 and (2^64-1), it is returned 
        /// as-is. Otherwise, if the value is a floating-point number in the 
        /// aforementioned range, it is truncated (rounded toward zero) and returned.<br/>
        /// 
        /// - If the value is a boolean, 1 is returned for true and 0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as an
        /// integer between 0 and (2^64-1), the parsed value is returned. The 
        /// string is parsed in a locale-independent way and does not support the use 
        /// of commas or other delimiters to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a UInt64.</returns>
        public partial UInt64 GetUInt64(Int64 featureID, UInt64 defaultValue = 0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a single-precision
        /// floating-point number.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is already a single-precision floating-point number, it is
        /// returned as-is.<br/>
        /// 
        /// - If the value is a scalar of any other type within the range of values 
        /// that a single-precision float can represent, it is converted to its closest
        /// representation as a single-precision float and returned.<br/>
        /// 
        /// - If the value is a boolean, 1.0f is returned for true and 0.0f for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as a number, 
        /// the parsed value is returned. The string is parsed in a
        /// locale-independent way and does not support the use of a comma or other
        /// delimiter to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Float.</returns>
        public partial float GetFloat(Int64 featureID, float defaultValue = 0.0f);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a double-precision
        /// floating-point number.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is already a single- or double-precision floating-point number,
        /// it is returned as-is.<br/>
        /// 
        /// - If the value is an integer, it is converted to the closest representable
        /// double-precision floating-point number.<br/>
        /// 
        /// - If the value is a boolean, 1.0 is returned for true and 0.0 for false.<br/>
        /// 
        /// - If the value is a string and the entire string can be parsed as a number, 
        /// the parsed value is returned. The string is parsed in a
        /// locale-independent way and does not support the use of a comma or other
        /// delimiter to group digits together.<br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a Double.</returns>
        public partial double GetDouble(Int64 featureID, double defaultValue = 0.0);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a int2.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is a 2-dimensional vector, its components will be converted 
        /// to 32-bit signed integers if possible.<br/>
        /// 
        /// - If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the Vector2Int.<br/>
        /// 
        /// - If the value is a scalar that can be converted to a 32-bit signed
        /// integer, the resulting Vector2Int will have this value in both of its
        /// components.<br/>
        /// 
        /// - If the value is a boolean, (1, 1) is returned for true, while (0, 0) is 
        /// returned for false.<br/>
        /// 
        /// - If the value is a string that can be parsed as an int2, the parsed 
        /// value is returned. The string must be formatted as "(X, Y)" or "int2(X, Y)".
        /// <br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. In all vector 
        /// cases, if any of the relevant components cannot be represented as a 32-bit signed
        /// integer, the default value is returned.<br/><br/>
        /// If the feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a int2.</returns>
        public partial int2 GetInt2(Int64 featureID, int2 defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a uint2.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is a 2-dimensional vector, its components will be converted 
        /// to 32-bit unsigned integers if possible.<br/>
        /// 
        /// - If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the uint2.<br/>
        /// 
        /// - If the value is a scalar that can be converted to a 32-bit unsigned
        /// integer, the resulting uint2 will have this value in both of its
        /// components.<br/>
        /// 
        /// - If the value is a boolean, (1, 1) is returned for true, while (0, 0) is 
        /// returned for false.<br/>
        /// 
        /// - If the value is a string that can be parsed as a uint2, the parsed 
        /// value is returned. The string must be formatted as "(X, Y)" or "uint(X, Y)".
        /// <br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. In all vector 
        /// cases, if any of the relevant components cannot be represented as a 32-bit 
        /// unsigned integer, the default value is returned.<br/><br/>
        /// If the feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a uint2.</returns>
        public partial uint2 GetUInt2(Int64 featureID, uint2 defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a float2.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is a 2-dimensional vector, its components will be converted 
        /// to the closest representable single-precision floats, if possible.<br/>
        /// 
        /// - If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the float2.<br/>
        /// 
        /// - If the value is a scalar that can be converted to a single-precision float,
        /// the resulting float2 will have this value in both of its components.<br/>
        /// 
        /// - If the value is a boolean, (1.0f, 1.0f) is returned for true, while 
        /// (0.0f, 0.0f) is returned for false.<br/>
        /// 
        /// - If the value is a string that can be parsed as a float2, the parsed 
        /// value is returned. The string must be formatted as "(X, Y)" or "float2(X, Y)".
        /// <br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. In all vector 
        /// cases, if any of the relevant components cannot be represented as a 
        /// single-precision floating point number, the default value is returned.<br/><br/>
        /// If the feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a float2.</returns>
        public partial float2 GetFloat2(Int64 featureID, float2 defaultValue);

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a double2.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it is
        /// further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// 
        /// - If the value is a 2-dimensional vector, its components will be converted 
        /// to double-precision floating-point numbers.<br/>
        /// 
        /// - If the value is a 3- or 4-dimensional vector, it will use the first two
        /// components to construct the double2.<br/>
        /// 
        /// - If the value is a scalar, the resulting float2 will have this value in
        /// both of its components.<br/>
        /// 
        /// - If the value is a boolean, (1.0, 1.0) is returned for true, while 
        /// (0.0, 0.0) is returned for false.<br/>
        /// 
        /// - If the value is a string that can be parsed as a double2, the parsed 
        /// value is returned. The string must be formatted as "(X, Y)" or 
        /// "double2(X, Y)".
        /// <br/><br/>
        /// </para>
        /// <para>
        /// In all other cases, the user-defined default value is returned. If the 
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a double2.</returns>
        public partial double2 GetDouble2(Int64 featureID, double2 defaultValue);

        // TODO
        // int3, uint3, Vector3, double3,
        // int4, uint4, Vector4, double4,
        // int2x2, uint2x2, float2x2, double2x2
        // int3x3, uint3x3, float3x3, double3x3,
        // int4x4, uint4x4, Matrix4x4, double4x4

        /// <summary>
        /// Attempts to retrieve the value for the given feature as a String.
        /// </summary>
        /// <para>
        /// For numeric properties, the raw value for a given feature will be
        /// transformed by the property's normalization, scale, and offset before it 
        /// is further converted. If the raw value is equal to the property's "no data"
        /// value, then the property's default value will be converted if possible. If
        /// the property-defined default value cannot be converted, or does not exist,
        /// then the user-defined default value is returned.
        /// </para>
        /// <para>
        /// Property values are converted as follows:<br/>
        /// - String properties are returned as-is.<br/>
        /// - Scalar values are converted to a string with `std::to_string`.<br/>
        /// - Boolean properties are converted to "true" or "false".<br/>
        /// </para>
        /// <para>
        /// If the
        /// feature ID is out-of-range, or if the property table property is somehow
        /// invalid, the user-defined default value is returned.
        /// </para>
        /// </remarks>
        /// <param name="featureID">The ID of the feature.</param>
        /// <param name="defaultValue">The default value to fall back on.</param>
        /// <returns>The property value as a String.</returns>
        public partial String GetString(Int64 featureID, String defaultValue = "");
        #endregion
    }
}
