using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Skobbler.SDKDemo.Util;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.settings);

            ListPreference listPreference = (ListPreference)FindPreference(PreferenceTypes.K_ROUTE_TYPE);

            listPreference.PreferenceChange += (s, e) =>
            {
                listPreference.Value = e.NewValue.ToString();
                e.Preference.Summary = listPreference.Entry;
                e.Handled = false;
            };

            ListPreference listDistanceFormat = (ListPreference)FindPreference(PreferenceTypes.K_DISTANCE_UNIT);
            if (listDistanceFormat.Value == null)
            {
                listDistanceFormat.SetValueIndex(0);
            }

            ListPreference listSpeedWarningsOutTown = (ListPreference)FindPreference(PreferenceTypes.K_OUT_TOWN_SPEED_WARNING);
            ListPreference listSpeedWarningsInTown = (ListPreference)FindPreference(PreferenceTypes.K_IN_TOWN_SPEED_WARNING);

            listDistanceFormat.PreferenceChange += (s, e) =>
            {// Set the value as the new value
                listDistanceFormat.Value = (e.NewValue.ToString());
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = (listDistanceFormat.Entry);
                if (e.Preference.Summary.Equals("Miles/Feet") || e.Preference.Summary.Equals("Miles/Yards"))
                {
                    listSpeedWarningsInTown.SetEntries(new string[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                    listSpeedWarningsInTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
                    listSpeedWarningsOutTown.SetEntries(new string[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                    listSpeedWarningsOutTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
                }
                else if (e.Preference.Summary.Equals("Kilometers/Meters"))
                {
                    listSpeedWarningsInTown.SetEntries(new string[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                    listSpeedWarningsInTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
                    listSpeedWarningsOutTown.SetEntries(new string[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                    listSpeedWarningsOutTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
                }

                e.Handled = false;
            };

            ListPreference listNavigationType = (ListPreference)FindPreference(PreferenceTypes.K_NAVIGATION_TYPE);
            if (listNavigationType.Value == null)
            {
                listNavigationType.SetValueIndex(1);
            }

            listNavigationType.PreferenceChange += (s, e) =>
            {// Set the value as the new value
                listNavigationType.Value = e.NewValue.ToString();
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = listNavigationType.Entry;
                e.Handled = false;
            };

            
            if (listDistanceFormat.Summary.Equals("Kilometers/Meters"))
            {
                listSpeedWarningsInTown.SetEntries(new string[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                listSpeedWarningsInTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            }
            else
            {
                listSpeedWarningsInTown.SetEntries(new string[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                listSpeedWarningsInTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            }
            if (listSpeedWarningsInTown.Value == null)
            {
                listSpeedWarningsInTown.SetValueIndex(3);
            }

            listSpeedWarningsInTown.PreferenceChange += (s, e) =>
            {// Set the value as the new value
                listSpeedWarningsInTown.Value = e.NewValue.ToString();
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = listSpeedWarningsInTown.Entry;
                e.Handled = false;
            };

            if (listDistanceFormat.Summary.Equals("Kilometers/Meters"))
            {
                listSpeedWarningsOutTown.SetEntries(new string[] { "5km/h", "10km/h", "15km/h", "20km/h", "25km/h" });
                listSpeedWarningsOutTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            }
            else
            {
                listSpeedWarningsOutTown.SetEntries(new string[] { "5mi/h", "10mi/h", "15mi/h", "20mi/h", "25mi/h" });
                listSpeedWarningsOutTown.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            }
            if (listSpeedWarningsOutTown.Value == null)
            {
                listSpeedWarningsOutTown.SetValueIndex(3);
            }

            listSpeedWarningsOutTown.PreferenceChange += (s, e) =>
            {// Set the value as the new value
                listSpeedWarningsOutTown.Value = (e.NewValue.ToString());
                // Get the entry which corresponds to the current value and set as summary
                e.Preference.Summary = listSpeedWarningsOutTown.Entry;
                e.Handled = false;
            };

            CheckBoxPreference checkBoxDayNight = (CheckBoxPreference)FindPreference(PreferenceTypes.K_AUTO_DAY_NIGHT);

            checkBoxDayNight.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxTollRoads = (CheckBoxPreference)FindPreference(PreferenceTypes.K_AVOID_TOLL_ROADS);
            checkBoxTollRoads.PreferenceChange += (s, e) =>
            {
                Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString());
                e.Handled = true;
            };

            CheckBoxPreference checkBoxFerries = (CheckBoxPreference)FindPreference(PreferenceTypes.K_AVOID_FERRIES);
            checkBoxFerries.PreferenceChange += (s, e) => { Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString()); };

            CheckBoxPreference checkBoxHighways = (CheckBoxPreference)FindPreference(PreferenceTypes.K_AVOID_HIGHWAYS);
            checkBoxHighways.PreferenceChange += (s, e) => { Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString()); };

            CheckBoxPreference checkBoxFreeDrive = (CheckBoxPreference)FindPreference(PreferenceTypes.K_FREE_DRIVE);
            checkBoxFreeDrive.PreferenceChange += (s, e) => { Log.Debug("MyApp", "Pref " + e.Preference.Key + " changed to " + e.NewValue.ToString()); };

        }

    }
}