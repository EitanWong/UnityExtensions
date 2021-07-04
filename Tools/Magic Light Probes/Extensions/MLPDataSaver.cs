using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MagicLightProbes
{
    public class MLPDataSaver
    { 
        /// <summary>Saves data to disk. Stored data must be supported by standard Unity serialization.</summary>
        /// <param name="data">Data to save.</param> 
        /// <param name="fullFilePath">File name.</param>
        /// <param name="consoleString">
        ///     Optional. String output to console on successful save. 
        ///     If not specified, the standard string will be displayed.
        ///     <para> If an error is detected, information about it will be displayed in the console. </para>
        /// </param>
        public static void SaveData<T>(T data, string fullFilePath, string consoleString = "")
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(fullFilePath, FileMode.Create);

                binaryFormatter.Serialize(fileStream, data);
                fileStream.Close();

                if (consoleString.Length > 0)
                {
                    Debug.Log(consoleString);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                fileStream.Close();
            }
        }

        /// <summary>Laod data from disk. If the data file is not found, a new file will be created and the current data from the recipient will be written to it.</summary>
        /// <param name="dataRecipient">Recipient of loaded data.</param> 
        /// <param name="fullFilePath">Data file name.</param>
        /// <param name="consoleStringSuccess">
        ///     Optional. String output to console on successful load. 
        ///     If not specified, the standard string will be displayed.
        ///     <para> If an error is detected, information about it will be displayed in the console. </para>
        /// </param>
        public static T LoadData<T>(T dataRecipient, string fullFilePath, string consoleStringSuccess = "", string consoleStringFail = "")
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = null;
            bool fileExsist = false;

            try
            {
                if (File.Exists(fullFilePath))
                {
                    fileStream = new FileStream(fullFilePath, FileMode.Open);
                    fileExsist = true;
                }

                if (fileExsist)
                {
                    dataRecipient = (T) binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();

                    if (consoleStringSuccess.Length > 0)
                    {
                        Debug.Log(consoleStringSuccess);
                    }

                    return dataRecipient;
                }
                else
                {
                    if (consoleStringFail.Length > 0)
                    {
                        Debug.Log(consoleStringSuccess);
                    }

                    return dataRecipient;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                fileStream.Close();

                return default(T);
            }
        }
    }
}
