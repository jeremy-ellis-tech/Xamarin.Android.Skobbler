using System;
using System.Collections.Generic;
using Android.Runtime;

namespace Skobbler.Ngx
{

    // Metadata.xml XPath class reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']"
    [global::Android.Runtime.Register("com/skobbler/ngx/SKMaps", DoNotGenerateAcw = true)]
    public sealed partial class SKMaps : Java.Lang.Object
    {


        // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/field[@name='CONNECTIVITY_MODE_OFFLINE']"
        [Register("CONNECTIVITY_MODE_OFFLINE")]
        public const sbyte ConnectivityModeOffline = (sbyte)2;

        // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/field[@name='CONNECTIVITY_MODE_ONLINE']"
        [Register("CONNECTIVITY_MODE_ONLINE")]
        public const sbyte ConnectivityModeOnline = (sbyte)1;

        static IntPtr updateToLatestSDKVersion_jfieldId;

        // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/field[@name='updateToLatestSDKVersion']"
        [Register("updateToLatestSDKVersion")]
        public static bool UpdateToLatestSDKVersion
        {
            get
            {
                if (updateToLatestSDKVersion_jfieldId == IntPtr.Zero)
                    updateToLatestSDKVersion_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "updateToLatestSDKVersion", "Z");
                return JNIEnv.GetStaticBooleanField(class_ref, updateToLatestSDKVersion_jfieldId);
            }
            set
            {
                if (updateToLatestSDKVersion_jfieldId == IntPtr.Zero)
                    updateToLatestSDKVersion_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "updateToLatestSDKVersion", "Z");
                try
                {
                    JNIEnv.SetStaticField(class_ref, updateToLatestSDKVersion_jfieldId, value);
                }
                finally
                {
                }
            }
        }
        // Metadata.xml XPath class reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']"
        [global::Android.Runtime.Register("com/skobbler/ngx/SKMaps$SKDistanceUnitType", DoNotGenerateAcw = true)]
        public sealed partial class SKDistanceUnitType : global::Java.Lang.Enum
        {


            static IntPtr DISTANCE_UNIT_KILOMETER_METERS_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/field[@name='DISTANCE_UNIT_KILOMETER_METERS']"
            [Register("DISTANCE_UNIT_KILOMETER_METERS")]
            public static global::Skobbler.Ngx.SKMaps.SKDistanceUnitType DistanceUnitKilometerMeters
            {
                get
                {
                    if (DISTANCE_UNIT_KILOMETER_METERS_jfieldId == IntPtr.Zero)
                        DISTANCE_UNIT_KILOMETER_METERS_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "DISTANCE_UNIT_KILOMETER_METERS", "Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, DISTANCE_UNIT_KILOMETER_METERS_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKDistanceUnitType>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr DISTANCE_UNIT_MILES_FEET_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/field[@name='DISTANCE_UNIT_MILES_FEET']"
            [Register("DISTANCE_UNIT_MILES_FEET")]
            public static global::Skobbler.Ngx.SKMaps.SKDistanceUnitType DistanceUnitMilesFeet
            {
                get
                {
                    if (DISTANCE_UNIT_MILES_FEET_jfieldId == IntPtr.Zero)
                        DISTANCE_UNIT_MILES_FEET_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "DISTANCE_UNIT_MILES_FEET", "Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, DISTANCE_UNIT_MILES_FEET_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKDistanceUnitType>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr DISTANCE_UNIT_MILES_YARDS_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/field[@name='DISTANCE_UNIT_MILES_YARDS']"
            [Register("DISTANCE_UNIT_MILES_YARDS")]
            public static global::Skobbler.Ngx.SKMaps.SKDistanceUnitType DistanceUnitMilesYards
            {
                get
                {
                    if (DISTANCE_UNIT_MILES_YARDS_jfieldId == IntPtr.Zero)
                        DISTANCE_UNIT_MILES_YARDS_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "DISTANCE_UNIT_MILES_YARDS", "Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, DISTANCE_UNIT_MILES_YARDS_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKDistanceUnitType>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }
            internal static IntPtr java_class_handle;
            internal static IntPtr class_ref
            {
                get
                {
                    return JNIEnv.FindClass("com/skobbler/ngx/SKMaps$SKDistanceUnitType", ref java_class_handle);
                }
            }

            protected override IntPtr ThresholdClass
            {
                get { return class_ref; }
            }

            protected override global::System.Type ThresholdType
            {
                get { return typeof(SKDistanceUnitType); }
            }

            internal SKDistanceUnitType(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

            static IntPtr id_getValue;
            public unsafe int Value
            {
                // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/method[@name='getValue' and count(parameter)=0]"
                [Register("getValue", "()I", "GetGetValueHandler")]
                get
                {
                    if (id_getValue == IntPtr.Zero)
                        id_getValue = JNIEnv.GetMethodID(class_ref, "getValue", "()I");
                    try
                    {
                        return JNIEnv.CallIntMethod(Handle, id_getValue);
                    }
                    finally
                    {
                    }
                }
            }

            static IntPtr id_forInt_I;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/method[@name='forInt' and count(parameter)=1 and parameter[1][@type='int']]"
            [Register("forInt", "(I)Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKDistanceUnitType ForInt(int id)
            {
                if (id_forInt_I == IntPtr.Zero)
                    id_forInt_I = JNIEnv.GetStaticMethodID(class_ref, "forInt", "(I)Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(id);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKDistanceUnitType>(JNIEnv.CallStaticObjectMethod(class_ref, id_forInt_I, __args), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }

            static IntPtr id_valueOf_Ljava_lang_String_;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/method[@name='valueOf' and count(parameter)=1 and parameter[1][@type='java.lang.String']]"
            [Register("valueOf", "(Ljava/lang/String;)Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKDistanceUnitType ValueOf(string name)
            {
                if (id_valueOf_Ljava_lang_String_ == IntPtr.Zero)
                    id_valueOf_Ljava_lang_String_ = JNIEnv.GetStaticMethodID(class_ref, "valueOf", "(Ljava/lang/String;)Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                IntPtr native_name = JNIEnv.NewString(name);
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(native_name);
                    global::Skobbler.Ngx.SKMaps.SKDistanceUnitType __ret = global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKDistanceUnitType>(JNIEnv.CallStaticObjectMethod(class_ref, id_valueOf_Ljava_lang_String_, __args), JniHandleOwnership.TransferLocalRef);
                    return __ret;
                }
                finally
                {
                    JNIEnv.DeleteLocalRef(native_name);
                }
            }

            static IntPtr id_values;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKDistanceUnitType']/method[@name='values' and count(parameter)=0]"
            [Register("values", "()[Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKDistanceUnitType[] Values()
            {
                if (id_values == IntPtr.Zero)
                    id_values = JNIEnv.GetStaticMethodID(class_ref, "values", "()[Lcom/skobbler/ngx/SKMaps$SKDistanceUnitType;");
                try
                {
                    return (global::Skobbler.Ngx.SKMaps.SKDistanceUnitType[])JNIEnv.GetArray(JNIEnv.CallStaticObjectMethod(class_ref, id_values), JniHandleOwnership.TransferLocalRef, typeof(global::Skobbler.Ngx.SKMaps.SKDistanceUnitType));
                }
                finally
                {
                }
            }

        }

        // Metadata.xml XPath class reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']"
        [global::Android.Runtime.Register("com/skobbler/ngx/SKMaps$SKLanguage", DoNotGenerateAcw = true)]
        public sealed partial class SKLanguage : global::Java.Lang.Enum
        {


            static IntPtr LANGUAGE_DE_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_DE']"
            [Register("LANGUAGE_DE")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageDe
            {
                get
                {
                    if (LANGUAGE_DE_jfieldId == IntPtr.Zero)
                        LANGUAGE_DE_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_DE", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_DE_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_EN_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_EN']"
            [Register("LANGUAGE_EN")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageEn
            {
                get
                {
                    if (LANGUAGE_EN_jfieldId == IntPtr.Zero)
                        LANGUAGE_EN_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_EN", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_EN_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_ES_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_ES']"
            [Register("LANGUAGE_ES")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageEs
            {
                get
                {
                    if (LANGUAGE_ES_jfieldId == IntPtr.Zero)
                        LANGUAGE_ES_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_ES", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_ES_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_FR_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_FR']"
            [Register("LANGUAGE_FR")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageFr
            {
                get
                {
                    if (LANGUAGE_FR_jfieldId == IntPtr.Zero)
                        LANGUAGE_FR_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_FR", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_FR_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_IT_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_IT']"
            [Register("LANGUAGE_IT")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageIt
            {
                get
                {
                    if (LANGUAGE_IT_jfieldId == IntPtr.Zero)
                        LANGUAGE_IT_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_IT", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_IT_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_LOCAL_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_LOCAL']"
            [Register("LANGUAGE_LOCAL")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageLocal
            {
                get
                {
                    if (LANGUAGE_LOCAL_jfieldId == IntPtr.Zero)
                        LANGUAGE_LOCAL_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_LOCAL", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_LOCAL_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_RU_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_RU']"
            [Register("LANGUAGE_RU")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageRu
            {
                get
                {
                    if (LANGUAGE_RU_jfieldId == IntPtr.Zero)
                        LANGUAGE_RU_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_RU", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_RU_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }

            static IntPtr LANGUAGE_TR_jfieldId;

            // Metadata.xml XPath field reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/field[@name='LANGUAGE_TR']"
            [Register("LANGUAGE_TR")]
            public static global::Skobbler.Ngx.SKMaps.SKLanguage LanguageTr
            {
                get
                {
                    if (LANGUAGE_TR_jfieldId == IntPtr.Zero)
                        LANGUAGE_TR_jfieldId = JNIEnv.GetStaticFieldID(class_ref, "LANGUAGE_TR", "Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                    IntPtr __ret = JNIEnv.GetStaticObjectField(class_ref, LANGUAGE_TR_jfieldId);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(__ret, JniHandleOwnership.TransferLocalRef);
                }
            }
            internal static IntPtr java_class_handle;
            internal static IntPtr class_ref
            {
                get
                {
                    return JNIEnv.FindClass("com/skobbler/ngx/SKMaps$SKLanguage", ref java_class_handle);
                }
            }

            protected override IntPtr ThresholdClass
            {
                get { return class_ref; }
            }

            protected override global::System.Type ThresholdType
            {
                get { return typeof(SKLanguage); }
            }

            internal SKLanguage(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

            static IntPtr id_getValue;
            public unsafe int Value
            {
                // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/method[@name='getValue' and count(parameter)=0]"
                [Register("getValue", "()I", "GetGetValueHandler")]
                get
                {
                    if (id_getValue == IntPtr.Zero)
                        id_getValue = JNIEnv.GetMethodID(class_ref, "getValue", "()I");
                    try
                    {
                        return JNIEnv.CallIntMethod(Handle, id_getValue);
                    }
                    finally
                    {
                    }
                }
            }

            static IntPtr id_forInt_I;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/method[@name='forInt' and count(parameter)=1 and parameter[1][@type='int']]"
            [Register("forInt", "(I)Lcom/skobbler/ngx/SKMaps$SKLanguage;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKLanguage ForInt(int id)
            {
                if (id_forInt_I == IntPtr.Zero)
                    id_forInt_I = JNIEnv.GetStaticMethodID(class_ref, "forInt", "(I)Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(id);
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(JNIEnv.CallStaticObjectMethod(class_ref, id_forInt_I, __args), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }

            static IntPtr id_valueOf_Ljava_lang_String_;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/method[@name='valueOf' and count(parameter)=1 and parameter[1][@type='java.lang.String']]"
            [Register("valueOf", "(Ljava/lang/String;)Lcom/skobbler/ngx/SKMaps$SKLanguage;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKLanguage ValueOf(string name)
            {
                if (id_valueOf_Ljava_lang_String_ == IntPtr.Zero)
                    id_valueOf_Ljava_lang_String_ = JNIEnv.GetStaticMethodID(class_ref, "valueOf", "(Ljava/lang/String;)Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                IntPtr native_name = JNIEnv.NewString(name);
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(native_name);
                    global::Skobbler.Ngx.SKMaps.SKLanguage __ret = global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps.SKLanguage>(JNIEnv.CallStaticObjectMethod(class_ref, id_valueOf_Ljava_lang_String_, __args), JniHandleOwnership.TransferLocalRef);
                    return __ret;
                }
                finally
                {
                    JNIEnv.DeleteLocalRef(native_name);
                }
            }

            static IntPtr id_values;
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps.SKLanguage']/method[@name='values' and count(parameter)=0]"
            [Register("values", "()[Lcom/skobbler/ngx/SKMaps$SKLanguage;", "")]
            public static unsafe global::Skobbler.Ngx.SKMaps.SKLanguage[] Values()
            {
                if (id_values == IntPtr.Zero)
                    id_values = JNIEnv.GetStaticMethodID(class_ref, "values", "()[Lcom/skobbler/ngx/SKMaps$SKLanguage;");
                try
                {
                    return (global::Skobbler.Ngx.SKMaps.SKLanguage[])JNIEnv.GetArray(JNIEnv.CallStaticObjectMethod(class_ref, id_values), JniHandleOwnership.TransferLocalRef, typeof(global::Skobbler.Ngx.SKMaps.SKLanguage));
                }
                finally
                {
                }
            }

        }

        internal static IntPtr java_class_handle;
        internal static IntPtr class_ref
        {
            get
            {
                return JNIEnv.FindClass("com/skobbler/ngx/SKMaps", ref java_class_handle);
            }
        }

        protected override IntPtr ThresholdClass
        {
            get { return class_ref; }
        }

        protected override global::System.Type ThresholdType
        {
            get { return typeof(SKMaps); }
        }

        internal SKMaps(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        static IntPtr id_getApiKey;
        static IntPtr id_setApiKey_Ljava_lang_String_;
        public unsafe string ApiKey
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getApiKey' and count(parameter)=0]"
            [Register("getApiKey", "()Ljava/lang/String;", "GetGetApiKeyHandler")]
            get
            {
                if (id_getApiKey == IntPtr.Zero)
                    id_getApiKey = JNIEnv.GetMethodID(class_ref, "getApiKey", "()Ljava/lang/String;");
                try
                {
                    return JNIEnv.GetString(JNIEnv.CallObjectMethod(Handle, id_getApiKey), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='setApiKey' and count(parameter)=1 and parameter[1][@type='java.lang.String']]"
            [Register("setApiKey", "(Ljava/lang/String;)V", "GetSetApiKey_Ljava_lang_String_Handler")]
            set
            {
                if (id_setApiKey_Ljava_lang_String_ == IntPtr.Zero)
                    id_setApiKey_Ljava_lang_String_ = JNIEnv.GetMethodID(class_ref, "setApiKey", "(Ljava/lang/String;)V");
                IntPtr native_value = JNIEnv.NewString(value);
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(native_value);
                    JNIEnv.CallVoidMethod(Handle, id_setApiKey_Ljava_lang_String_, __args);
                }
                finally
                {
                    JNIEnv.DeleteLocalRef(native_value);
                }
            }
        }

        static IntPtr id_getConnectivityMode;
        static IntPtr id_setConnectivityMode_B;
        public unsafe sbyte ConnectivityMode
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getConnectivityMode' and count(parameter)=0]"
            [Register("getConnectivityMode", "()B", "GetGetConnectivityModeHandler")]
            get
            {
                if (id_getConnectivityMode == IntPtr.Zero)
                    id_getConnectivityMode = JNIEnv.GetMethodID(class_ref, "getConnectivityMode", "()B");
                try
                {
                    return JNIEnv.CallByteMethod(Handle, id_getConnectivityMode);
                }
                finally
                {
                }
            }
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='setConnectivityMode' and count(parameter)=1 and parameter[1][@type='byte']]"
            [Register("setConnectivityMode", "(B)V", "GetSetConnectivityMode_BHandler")]
            set
            {
                if (id_setConnectivityMode_B == IntPtr.Zero)
                    id_setConnectivityMode_B = JNIEnv.GetMethodID(class_ref, "setConnectivityMode", "(B)V");
                try
                {
                    JValue* __args = stackalloc JValue[1];
                    __args[0] = new JValue(value);
                    JNIEnv.CallVoidMethod(Handle, id_setConnectivityMode_B, __args);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_getInstance;
        public static unsafe global::Skobbler.Ngx.SKMaps Instance
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getInstance' and count(parameter)=0]"
            [Register("getInstance", "()Lcom/skobbler/ngx/SKMaps;", "GetGetInstanceHandler")]
            get
            {
                if (id_getInstance == IntPtr.Zero)
                    id_getInstance = JNIEnv.GetStaticMethodID(class_ref, "getInstance", "()Lcom/skobbler/ngx/SKMaps;");
                try
                {
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMaps>(JNIEnv.CallStaticObjectMethod(class_ref, id_getInstance), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_isSKMapsInitialized;
        public unsafe bool IsSKMapsInitialized
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='isSKMapsInitialized' and count(parameter)=0]"
            [Register("isSKMapsInitialized", "()Z", "GetIsSKMapsInitializedHandler")]
            get
            {
                if (id_isSKMapsInitialized == IntPtr.Zero)
                    id_isSKMapsInitialized = JNIEnv.GetMethodID(class_ref, "isSKMapsInitialized", "()Z");
                try
                {
                    return JNIEnv.CallBooleanMethod(Handle, id_isSKMapsInitialized);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_getMapInitSettings;
        public unsafe global::Skobbler.Ngx.SKMapsInitSettings MapInitSettings
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getMapInitSettings' and count(parameter)=0]"
            [Register("getMapInitSettings", "()Lcom/skobbler/ngx/SKMapsInitSettings;", "GetGetMapInitSettingsHandler")]
            get
            {
                if (id_getMapInitSettings == IntPtr.Zero)
                    id_getMapInitSettings = JNIEnv.GetMethodID(class_ref, "getMapInitSettings", "()Lcom/skobbler/ngx/SKMapsInitSettings;");
                try
                {
                    return global::Java.Lang.Object.GetObject<global::Skobbler.Ngx.SKMapsInitSettings>(JNIEnv.CallObjectMethod(Handle, id_getMapInitSettings), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_getObfuscatedApiKey;
        public unsafe string ObfuscatedApiKey
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getObfuscatedApiKey' and count(parameter)=0]"
            [Register("getObfuscatedApiKey", "()Ljava/lang/String;", "GetGetObfuscatedApiKeyHandler")]
            get
            {
                if (id_getObfuscatedApiKey == IntPtr.Zero)
                    id_getObfuscatedApiKey = JNIEnv.GetMethodID(class_ref, "getObfuscatedApiKey", "()Ljava/lang/String;");
                try
                {
                    return JNIEnv.GetString(JNIEnv.CallObjectMethod(Handle, id_getObfuscatedApiKey), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_getSDKVersion;
        public unsafe string SDKVersion
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getSDKVersion' and count(parameter)=0]"
            [Register("getSDKVersion", "()Ljava/lang/String;", "GetGetSDKVersionHandler")]
            get
            {
                if (id_getSDKVersion == IntPtr.Zero)
                    id_getSDKVersion = JNIEnv.GetMethodID(class_ref, "getSDKVersion", "()Ljava/lang/String;");
                try
                {
                    return JNIEnv.GetString(JNIEnv.CallObjectMethod(Handle, id_getSDKVersion), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_getUserId;
        public unsafe string UserId
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='getUserId' and count(parameter)=0]"
            [Register("getUserId", "()Ljava/lang/String;", "GetGetUserIdHandler")]
            get
            {
                if (id_getUserId == IntPtr.Zero)
                    id_getUserId = JNIEnv.GetMethodID(class_ref, "getUserId", "()Ljava/lang/String;");
                try
                {
                    return JNIEnv.GetString(JNIEnv.CallObjectMethod(Handle, id_getUserId), JniHandleOwnership.TransferLocalRef);
                }
                finally
                {
                }
            }
        }

        static IntPtr id_destroySKMaps;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='destroySKMaps' and count(parameter)=0]"
        [Register("destroySKMaps", "()V", "")]
        public unsafe void DestroySKMaps()
        {
            if (id_destroySKMaps == IntPtr.Zero)
                id_destroySKMaps = JNIEnv.GetMethodID(class_ref, "destroySKMaps", "()V");
            try
            {
                JNIEnv.CallVoidMethod(Handle, id_destroySKMaps);
            }
            finally
            {
            }
        }

        static IntPtr id_enableProxy_Z;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='enableProxy' and count(parameter)=1 and parameter[1][@type='boolean']]"
        [Register("enableProxy", "(Z)V", "")]
        public unsafe void EnableProxy(bool enable)
        {
            if (id_enableProxy_Z == IntPtr.Zero)
                id_enableProxy_Z = JNIEnv.GetMethodID(class_ref, "enableProxy", "(Z)V");
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(enable);
                JNIEnv.CallVoidMethod(Handle, id_enableProxy_Z, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_finalizeLibrary;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='finalizeLibrary' and count(parameter)=0]"
        [Register("finalizeLibrary", "()Z", "")]
        public unsafe bool FinalizeLibrary()
        {
            if (id_finalizeLibrary == IntPtr.Zero)
                id_finalizeLibrary = JNIEnv.GetMethodID(class_ref, "finalizeLibrary", "()Z");
            try
            {
                return JNIEnv.CallBooleanMethod(Handle, id_finalizeLibrary);
            }
            finally
            {
            }
        }

        static IntPtr id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='initializeSKMaps' and count(parameter)=2 and parameter[1][@type='android.app.Application'] and parameter[2][@type='com.skobbler.ngx.SKMapsInitializationListener']]"
        [Register("initializeSKMaps", "(Landroid/app/Application;Lcom/skobbler/ngx/SKMapsInitializationListener;)V", "")]
        public unsafe void InitializeSKMaps(global::Android.App.Application context, global::Skobbler.Ngx.ISKMapsInitializationListener mapsInitListener)
        {
            if (id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_ == IntPtr.Zero)
                id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_ = JNIEnv.GetMethodID(class_ref, "initializeSKMaps", "(Landroid/app/Application;Lcom/skobbler/ngx/SKMapsInitializationListener;)V");
            try
            {
                JValue* __args = stackalloc JValue[2];
                __args[0] = new JValue(context);
                __args[1] = new JValue(mapsInitListener);
                JNIEnv.CallVoidMethod(Handle, id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_Lcom_skobbler_ngx_SKMapsInitSettings_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='initializeSKMaps' and count(parameter)=3 and parameter[1][@type='android.app.Application'] and parameter[2][@type='com.skobbler.ngx.SKMapsInitializationListener'] and parameter[3][@type='com.skobbler.ngx.SKMapsInitSettings']]"
        [Register("initializeSKMaps", "(Landroid/app/Application;Lcom/skobbler/ngx/SKMapsInitializationListener;Lcom/skobbler/ngx/SKMapsInitSettings;)V", "")]
        public unsafe void InitializeSKMaps(global::Android.App.Application context, global::Skobbler.Ngx.ISKMapsInitializationListener mapsInitListener, global::Skobbler.Ngx.SKMapsInitSettings mapInitSettings)
        {
            if (id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_Lcom_skobbler_ngx_SKMapsInitSettings_ == IntPtr.Zero)
                id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_Lcom_skobbler_ngx_SKMapsInitSettings_ = JNIEnv.GetMethodID(class_ref, "initializeSKMaps", "(Landroid/app/Application;Lcom/skobbler/ngx/SKMapsInitializationListener;Lcom/skobbler/ngx/SKMapsInitSettings;)V");
            try
            {
                JValue* __args = stackalloc JValue[3];
                __args[0] = new JValue(context);
                __args[1] = new JValue(mapsInitListener);
                __args[2] = new JValue(mapInitSettings);
                JNIEnv.CallVoidMethod(Handle, id_initializeSKMaps_Landroid_app_Application_Lcom_skobbler_ngx_SKMapsInitializationListener_Lcom_skobbler_ngx_SKMapsInitSettings_, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_onMapTexturesPrepared_Z;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='onMapTexturesPrepared' and count(parameter)=1 and parameter[1][@type='boolean']]"
        [Register("onMapTexturesPrepared", "(Z)V", "")]
        public unsafe void OnMapTexturesPrepared(bool texturesPrepared)
        {
            if (id_onMapTexturesPrepared_Z == IntPtr.Zero)
                id_onMapTexturesPrepared_Z = JNIEnv.GetMethodID(class_ref, "onMapTexturesPrepared", "(Z)V");
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(texturesPrepared);
                JNIEnv.CallVoidMethod(Handle, id_onMapTexturesPrepared_Z, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_setDownloadListener_Lcom_skobbler_ngx_SKMapsDownloadListener_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='setDownloadListener' and count(parameter)=1 and parameter[1][@type='com.skobbler.ngx.SKMapsDownloadListener']]"
        [Register("setDownloadListener", "(Lcom/skobbler/ngx/SKMapsDownloadListener;)V", "")]
        public unsafe void SetDownloadListener(global::Skobbler.Ngx.ISKMapsDownloadListener downloadListener)
        {
            if (id_setDownloadListener_Lcom_skobbler_ngx_SKMapsDownloadListener_ == IntPtr.Zero)
                id_setDownloadListener_Lcom_skobbler_ngx_SKMapsDownloadListener_ = JNIEnv.GetMethodID(class_ref, "setDownloadListener", "(Lcom/skobbler/ngx/SKMapsDownloadListener;)V");
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(downloadListener);
                JNIEnv.CallVoidMethod(Handle, id_setDownloadListener_Lcom_skobbler_ngx_SKMapsDownloadListener_, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_setOnlineConnectionNotificationDelay_I;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='setOnlineConnectionNotificationDelay' and count(parameter)=1 and parameter[1][@type='int']]"
        [Register("setOnlineConnectionNotificationDelay", "(I)V", "")]
        public unsafe void SetOnlineConnectionNotificationDelay(int delay)
        {
            if (id_setOnlineConnectionNotificationDelay_I == IntPtr.Zero)
                id_setOnlineConnectionNotificationDelay_I = JNIEnv.GetMethodID(class_ref, "setOnlineConnectionNotificationDelay", "(I)V");
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(delay);
                JNIEnv.CallVoidMethod(Handle, id_setOnlineConnectionNotificationDelay_I, __args);
            }
            finally
            {
            }
        }

        static IntPtr id_setProxy_Lcom_skobbler_ngx_SKProxySettings_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='setProxy' and count(parameter)=1 and parameter[1][@type='com.skobbler.ngx.SKProxySettings']]"
        [Register("setProxy", "(Lcom/skobbler/ngx/SKProxySettings;)Z", "")]
        public unsafe bool SetProxy(global::Skobbler.Ngx.SKProxySettings proxySettings)
        {
            if (id_setProxy_Lcom_skobbler_ngx_SKProxySettings_ == IntPtr.Zero)
                id_setProxy_Lcom_skobbler_ngx_SKProxySettings_ = JNIEnv.GetMethodID(class_ref, "setProxy", "(Lcom/skobbler/ngx/SKProxySettings;)Z");
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(proxySettings);
                bool __ret = JNIEnv.CallBooleanMethod(Handle, id_setProxy_Lcom_skobbler_ngx_SKProxySettings_, __args);
                return __ret;
            }
            finally
            {
            }
        }

        static IntPtr id_unzipFile_Ljava_lang_String_Ljava_lang_String_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.skobbler.ngx']/class[@name='SKMaps']/method[@name='unzipFile' and count(parameter)=2 and parameter[1][@type='java.lang.String'] and parameter[2][@type='java.lang.String']]"
        [Register("unzipFile", "(Ljava/lang/String;Ljava/lang/String;)I", "")]
        public unsafe int UnzipFile(string sourceName, string destinationFolder)
        {
            if (id_unzipFile_Ljava_lang_String_Ljava_lang_String_ == IntPtr.Zero)
                id_unzipFile_Ljava_lang_String_Ljava_lang_String_ = JNIEnv.GetMethodID(class_ref, "unzipFile", "(Ljava/lang/String;Ljava/lang/String;)I");
            IntPtr native_sourceName = JNIEnv.NewString(sourceName);
            IntPtr native_destinationFolder = JNIEnv.NewString(destinationFolder);
            try
            {
                JValue* __args = stackalloc JValue[2];
                __args[0] = new JValue(native_sourceName);
                __args[1] = new JValue(native_destinationFolder);
                int __ret = JNIEnv.CallIntMethod(Handle, id_unzipFile_Ljava_lang_String_Ljava_lang_String_, __args);
                return __ret;
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_sourceName);
                JNIEnv.DeleteLocalRef(native_destinationFolder);
            }
        }

        #region "Event implementation for Skobbler.Ngx.ISKMapsDownloadListener"
        public event EventHandler DownloadFailed
        {
            add
            {
                global::Java.Interop.EventHelper.AddEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        __CreateISKMapsDownloadListenerImplementor,
                        SetDownloadListener,
                        __h => __h.OnDownloadFailedHandler += value);
            }
            remove
            {
                global::Java.Interop.EventHelper.RemoveEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor.__IsEmpty,
                        __v => SetDownloadListener(null),
                        __h => __h.OnDownloadFailedHandler -= value);
            }
        }

        public event EventHandler DownloadFinished
        {
            add
            {
                global::Java.Interop.EventHelper.AddEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        __CreateISKMapsDownloadListenerImplementor,
                        SetDownloadListener,
                        __h => __h.OnDownloadFinishedHandler += value);
            }
            remove
            {
                global::Java.Interop.EventHelper.RemoveEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor.__IsEmpty,
                        __v => SetDownloadListener(null),
                        __h => __h.OnDownloadFinishedHandler -= value);
            }
        }

        public event EventHandler Downloading
        {
            add
            {
                global::Java.Interop.EventHelper.AddEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        __CreateISKMapsDownloadListenerImplementor,
                        SetDownloadListener,
                        __h => __h.OnDownloadingHandler += value);
            }
            remove
            {
                global::Java.Interop.EventHelper.RemoveEventHandler<global::Skobbler.Ngx.ISKMapsDownloadListener, global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor>(
                        ref weak_implementor_SetDownloadListener,
                        global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor.__IsEmpty,
                        __v => SetDownloadListener(null),
                        __h => __h.OnDownloadingHandler -= value);
            }
        }

        WeakReference weak_implementor_SetDownloadListener;

        global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor __CreateISKMapsDownloadListenerImplementor()
        {
            return new global::Skobbler.Ngx.ISKMapsDownloadListenerImplementor(this);
        }
        #endregion
    }
}
