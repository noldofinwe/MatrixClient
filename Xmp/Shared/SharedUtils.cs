using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Avalonia.Threading;
using Logging;

namespace Shared
{
    public static class SharedUtils
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        /// <summary>
        /// Returns a hex string representing an unique device ID.
        /// The device ID is a SHA256 hash, hex string of the actual device ID XOR a device nonce to prevent tracking between apps.
        /// </summary>
        public static string GetUniqueDeviceId()
        {
            // Step 1: Gather platform-specific identifiers
            string rawId = GetHardwareFingerprint();

            // Step 2: Add a nonce for extra entropy
            byte[] nonce = GetDeviceNonce();

            // Step 3: Combine and hash
            byte[] rawBytes = Encoding.UTF8.GetBytes(rawId);
            byte[] combined = rawBytes.Length >= 32 ? XorShorten(rawBytes, nonce) : nonce;

            using SHA256 sha = SHA256.Create();
            byte[] hashed = sha.ComputeHash(combined);

            return ByteArrayToHexString(hashed);
        }

        private static string GetHardwareFingerprint()
        {
            // You can customize this based on platform
            string os = RuntimeInformation.OSDescription;
            string arch = RuntimeInformation.OSArchitecture.ToString();
            string machineName = Environment.MachineName;

            return $"{os}-{arch}-{machineName}";
        }

        private static byte[] GetDeviceNonce()
        {
            byte[] nonce = new byte[32];
            RandomNumberGenerator.Fill(nonce);
            return nonce;
        }
        

       
        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Calls the UI thread dispatcher and executes the given callback on it.
        /// </summary>
        /// <param name="callback">The callback that should be executed in the UI thread.</param>
        public static async Task CallDispatcherAsync(Action callback)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                callback();
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(callback);
            }
        }

        /// <summary>
        /// Retries the given action once on exception.
        /// </summary>
        /// <param name="action">The action that should get executed.</param>
        public static void RetryOnException(Action action)
        {
            RetryOnException(action, 1);
        }

        /// <summary>
        /// Retries the given action retryCount times on exception.
        /// </summary>
        /// <param name="action">The action that should get executed.</param>
        /// <param name="retryCount">How many times should we try to execute the given action?</param>
        public static void RetryOnException(Action action, int retryCount)
        {
            int i = 0;
            do
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    if (retryCount <= i)
                    {
                        throw e;
                    }
                    else
                    {
                        Logger.Error("Retry exception: ", e);
                        i++;
                    }
                }
            } while (true);
        }

        /// <summary>
        /// Retries the given function once on exception.
        /// </summary>
        /// <param name="funct">The function that should get executed.</param>
        /// <param name="retryCount">How many times should we try to execute the given action?</param>
        /// <returns>The return value of the given function.</returns>
        public static T RetryOnException<T>(Func<T> funct)
        {
            return RetryOnException(funct, 1);
        }

        /// <summary>
        /// Retries the given function retryCount times on exception.
        /// </summary>
        /// <param name="funct">The function that should get executed.</param>
        /// <param name="retryCount">How many times should we try to execute the given action?</param>
        /// <returns>The return value of the given function.</returns>
        public static T RetryOnException<T>(Func<T> funct, int retryCount)
        {
            int i = 0;
            do
            {
                try
                {
                    return funct();
                }
                catch (Exception e)
                {
                    if (retryCount <= i)
                    {
                        throw e;
                    }
                    else
                    {
                        Logger.Error("Retry exception: ", e);
                        i++;
                    }
                }
            } while (true);
        }

        /// <summary>
        /// Creates a MediaPlayer object and plays the given sound.
        /// </summary>
        /// <param name="s">The URI sound path.</param>
        /// <returns>The created MediaPlayer object, that plays the requested sound.</returns>
        // public static MediaPlayer PlaySoundFromUri(string uri)
        // {
        //     MediaPlayer player = new MediaPlayer()
        //     {
        //         Source = MediaSource.CreateFromUri(new Uri(uri))
        //     };
        //     player.Play();
        //     return player;
        // }

        /// <summary>
        /// Vibrates the device for the given timespan if the device supports phone vibration.
        /// </summary>
        /// <param name="duration">How long should the vibration persist. Max 5 seconds.</param>
        // public static void VibratePress(TimeSpan duration)
        // {
        //     if (DeviceFamilyHelper.SupportsVibration())
        //     {
        //       //  VibrationDevice.GetDefault().Vibrate(duration);
        //     }
        //     Logger.Debug("Vibration not supported.");
        // }

        #endregion

        #region --Misc Methods (Private)--
        private static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Calculates the a XOR b.
        /// The result has the length of the shorter array.
        /// </summary>
        /// <param name="a">First input byte array.</param>
        /// <param name="b">Second input byte array.</param>
        /// <returns>a XOR b with the length of the shorter array.</returns>
        private static byte[] XorShorten(byte[] a, byte[] b)
        {
            int len = a.Length;
            if (b.Length < len)
            {
                len = b.Length;
            }

            byte[] result = new byte[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }
            return result;
        }

        /// <summary>
        /// Converts the given byte array to a hex-string and returns it.
        /// </summary>
        public static string ToHexString(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Converts the given hex-string to a byte array and returns it.
        /// </summary>
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
