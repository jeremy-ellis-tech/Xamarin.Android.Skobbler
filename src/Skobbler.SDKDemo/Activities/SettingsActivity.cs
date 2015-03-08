using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Skobbler.SDKDemo.Util;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.settings);

            ListPreference listPreference = (ListPreference)FindPreference(PreferenceTypes.KRouteType);

            listPreference.PreferenceChange += (s, e) =>
            {
                // Set the value as the new value
                listPreference.Value = e.NewValue.ToString();

                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = listPreference.Entry;

                if (e.Preference.Summary.Equals("Car shortest"))
                {
                    PreferenceScreen.FindPreference("pref_routes_number").Summary = "1";
                    PreferenceScreen.FindPreference("pref_routes_number").Enabled = false;
                }
                else if (e.Preference.Summary.Equals("Bicycle shortest"))
                {
                    PreferenceScreen.FindPreference("pref_routes_number").Summary = "1";
                    PreferenceScreen.FindPreference("pref_routes_number").Enabled = false;
                }

                e.Handled = false;
            };

            ListPreference listDistanceFormat = (ListPreference)FindPreference(PreferenceTypes.KDistanceUnit);
            if (listDistanceFormat.Value == null)
            {
                listDistanceFormat.SetValueIndex(0);
            }

            listDistanceFormat.PreferenceChange += (s, e) =>
                {
                    // Set the value as the new value
                    listDistanceFormat.Value = e.NewValue.ToString();
                    // Get the entry which corresponds to the current value and set as summary
                    e.Preference.Summary = listDistanceFormat.Entry;
                    if (e.Preference.Summary.Equals("Miles/Feet") || e.Preference.Summary.Equals("Miles/Yards"))
                    {

                        ListPreference listSpeedWarningsInTown = (ListPreference)FindPreference("pref_speed_warnings_in_town");
                        listSpeedWarningsInTown.SetEntries(new[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                        listSpeedWarningsInTown.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
                        ListPreference listSpeedWarningsOutTown = (ListPreference)FindPreference("pref_speed_warnings_out_town");
                        listSpeedWarningsOutTown.SetEntries(new[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                        listSpeedWarningsOutTown.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
                    }
                    else if (e.Preference.Summary.Equals("Kilometers/Meters"))
                    {
                        ListPreference listSpeedWarningsInTown = (ListPreference)FindPreference("pref_speed_warnings_in_town");
                        listSpeedWarningsInTown.SetEntries(new[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                        listSpeedWarningsInTown.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
                        ListPreference listSpeedWarningsOutTown = (ListPreference)FindPreference("pref_speed_warnings_out_town");
                        listSpeedWarningsOutTown.SetEntries(new[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                        listSpeedWarningsOutTown.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
                    }
                    e.Handled = false;
                };

            ListPreference listNavigationType = (ListPreference)FindPreference(PreferenceTypes.KNavigationType);

            if (listNavigationType.Value == null)
            {
                listNavigationType.SetValueIndex(1);
            }

            listNavigationType.PreferenceChange += (s, e) =>
            {
                // Set the value as the new value
                listNavigationType.Value = e.NewValue.ToString();
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = listNavigationType.Entry;

                e.Handled = false;
            };

            ListPreference lswit = (ListPreference)FindPreference(PreferenceTypes.KInTownSpeedWarning);

            if (listDistanceFormat.Summary.Equals("Kilometers/Meters"))
            {
                lswit.SetEntries(new[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                lswit.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
            }
            else
            {
                lswit.SetEntries(new[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                lswit.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
            }

            if (lswit.Value == null)
            {
                lswit.SetValueIndex(3);
            }

            lswit.PreferenceChange += (s, e) =>
            {
                // Set the value as the new value
                lswit.Value = e.NewValue.ToString();
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = lswit.Entry;
                e.Handled = false;
            };

            ListPreference lswot = (ListPreference)FindPreference(PreferenceTypes.KOutTownSpeedWarning);
            if (listDistanceFormat.Summary.Equals("Kilometers/Meters"))
            {
                lswot.SetEntries(new[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                lswot.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
            }
            else
            {
                lswot.SetEntries(new[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                lswot.SetEntryValues(new[] { "0", "1", "2", "3", "4" });
            }

            if (lswot.Value == null)
            {
                lswot.SetValueIndex(3);
            }

            lswot.PreferenceChange += (s, e) =>
            {
                // Set the value as the new value
                lswot.Value = e.NewValue.ToString();
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = lswot.Entry;
                e.Handled = false;
            };

            CheckBoxPreference checkBoxDayNight = (CheckBoxPreference)FindPreference(PreferenceTypes.KAutoDayNight);
            checkBoxDayNight.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxTollRoads = (CheckBoxPreference)FindPreference(PreferenceTypes.KAvoidTollRoads);
            checkBoxTollRoads.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxFerries = (CheckBoxPreference)FindPreference(PreferenceTypes.KAvoidFerries);
            checkBoxFerries.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxHighways = (CheckBoxPreference)FindPreference(PreferenceTypes.KAvoidHighways);
            checkBoxHighways.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxFreeDrive = (CheckBoxPreference)FindPreference(PreferenceTypes.KFreeDrive);
            checkBoxFreeDrive.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

        }

    }

}