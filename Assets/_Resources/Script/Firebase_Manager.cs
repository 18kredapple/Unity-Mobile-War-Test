using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace FireBase
{
    // Firebase_Manager.cs
    // Handles all Firebase-related operations: Auth, Firestore, Cloud Storage, Auto-Matchmaking
    public class Firebase_Manager : MonoBehaviour
    {
        public static Firebase_Manager instance { get; private set; }
        // Game1_Manager _game_Manager;
        // public Game1_Manager game_Manager
        // {
        //     get
        //     {
        //         if (_game_Manager == null)
        //             _game_Manager = FindObjectOfType<Game1_Manager>();
        //         return _game_Manager;
        //     }
        // }

        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        // TIP: leave MyStorageBucket without trailing slash; we normalize when joining.
        protected string MyStorageBucket = "gs://powerup-pool-22df2.appspot.com"; // FIX: typical bucket suffix is appspot.com
        protected string MyDatabaseUrl = "https://powerup-pool-22df2-default-rtdb.europe-west1.firebasedatabase.app"; // FIX: typical bucket suffix is appspot.com
        const int kMaxLogSize = 16382;
        protected static string UriFileScheme = Uri.UriSchemeFile + "://";
        string logText = "";

        [HideInInspector] public byte[] downloadData;

        protected string previousFileContents;
        protected string editableFileContents;
        protected string fileMetadata = "";
        protected string localFilename;
        public bool isFirebaseInitialized = false;
        protected FirebaseStorage storage;
        protected Firebase.LogLevel logLevel = Firebase.LogLevel.Info;
        protected bool operationInProgress;
        protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        protected string persistentDataPath;
        protected Task previousTask;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.Log("Error : Multiple instances of Firebase_Manager detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // FIX: set persistentDataPath on main thread
            persistentDataPath = Application.persistentDataPath;
        }

        void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                    InitializeFirebase();
                else
                    Debug.Log("Error : Could not resolve all Firebase dependencies: " + dependencyStatus);
            });
        }

        void Update() { }

        #region ** Firebase Auth **
        #endregion

        #region Initialize
        void InitializeFirebase()
        {
            // Initialize Storage
            var appBucket = FirebaseApp.DefaultInstance?.Options?.StorageBucket;
            storage = FirebaseStorage.DefaultInstance;

            if (!string.IsNullOrEmpty(appBucket))
            {
                // FIX: do not add trailing slash here; we normalize when building full paths
                MyStorageBucket = $"gs://{appBucket}";
            }

            storage.LogLevel = logLevel;
            isFirebaseInitialized = true;
            Debug.Log($"Firebase initialized. Bucket: {MyStorageBucket}");
        }
        #endregion

        #region Debug Log
        public void DebugLog(string s)
        {
            Debug.Log(s);
            logText += s + "\n";

            while (logText.Length > kMaxLogSize)
            {
                int index = logText.IndexOf("\n", StringComparison.Ordinal);
                if (index < 0) break;
                logText = logText.Substring(index + 1);
            }
        }
        #endregion

        #region  Does Document Exist
        public async Task<bool> DoesDocumentExist(string documentId)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("Users").Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        #endregion

        #region Get Document Info
        public async Task<Dictionary<string, object>> Get_Document(string collectionId, string documentId)
        {
            if (string.IsNullOrEmpty(documentId) || string.IsNullOrEmpty(collectionId))
            {
                Debug.Log("Error : Info fields is null or empty. Cannot get data.");
                return null;
            }

            Debug.Log("Getting user data...");
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection(collectionId).Document(documentId);
            try
            {
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    Dictionary<string, object> documentData = snapshot.ToDictionary();
                    return documentData;
                }
                else
                {
                    Debug.Log("Error : User data not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Firestore Exception: " + ex.Message);
                return null;
            }
        }
        #endregion

        #region Get Specific Data Info
        public async Task<object> Get_SpecificDataFromFirebase(string collectionId, string documentId, string path)
        {
            if (string.IsNullOrEmpty(documentId) || string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(path))
            {
                Debug.Log("Error : Info fields is null or empty. Cannot get data.");
                return null;
            }

            Debug.Log($"Getting data from {collectionId}/{documentId}, field: {path}");
            try
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                DocumentReference docRef = db.Collection(collectionId).Document(documentId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField(path))
                    {
                        object value = snapshot.GetValue<object>(path);
                        Debug.Log($"Field '{path}' value: {value}");
                        return value;
                    }
                    else
                    {
                        Debug.Log($"Error : Field '{path}' not found in document.");
                        return null;
                    }
                }
                else
                {
                    Debug.Log("Error : Document not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firestore Exception in Get_SpecificDataFromFirebase: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Get Specific Data Dictionary
        public async Task<Dictionary<string, object>> Get_SpecificDataDictionary(string collectionId, string documentId, string path)
        {
            if (string.IsNullOrEmpty(documentId) || string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(path))
            {
                Debug.Log("Error : Info fields is null or empty. Cannot get data.");
                return null;
            }

            Debug.Log($"Getting data from {collectionId}/{documentId}, field: {path}");
            try
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                DocumentReference docRef = db.Collection(collectionId).Document(documentId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField(path))
                    {
                        object value = snapshot.GetValue<object>(path);
                        Debug.Log($"Field '{path}' value: {value}");
                        return new Dictionary<string, object> { { path, value } };
                    }
                    else
                    {
                        Debug.Log($"Error : Field '{path}' not found in document.");
                        return null;
                    }
                }
                else
                {
                    Debug.Log("Error : Document not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firestore Exception in Get_SpecificDataDictionary: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Update Data Info
        public void Update_DatabaseField(string path, object value, string collectionId, string documentId)
        {
            if (string.IsNullOrEmpty(documentId) || string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(path))
            {
                Debug.Log("Error : Info fields is null or empty. Cannot get data.");
                return;
            }

            try
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                DocumentReference docRef = db.Collection(collectionId).Document(documentId);

                Dictionary<string, object> update = new Dictionary<string, object> { { path, value } };
                docRef.UpdateAsync(update).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                        Debug.Log("Document successfully updated.");
                    else
                        Debug.Log("Error : Failed to update document: " + task.Exception);
                });
            }
            catch (Exception ex)
            {
                Debug.Log("Error : Exception in Update_DatabaseField: " + ex.Message);
            }
        }
        #endregion

        #region Update Player Infos
        public void Update_DatabaseFields(Dictionary<string, object> fields, string collectionName = "Users")
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection(collectionName).Document(PlayerPrefs.GetString("PLAYER.uid"));

            docRef.UpdateAsync(fields).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                    Debug.Log("Document successfully updated.");
                else
                    Debug.Log("Error : Failed to update document: " + task.Exception);
            });
        }
        #endregion

        #region Delete Document
        async Task DeleteDocumentAsync(DocumentReference docRef)
        {
            try
            {
                await docRef.DeleteAsync();
                Debug.Log("Document deleted successfully!");
            }
            catch (Exception e)
            {
                Debug.Log("Error : Error deleting document: " + e.Message);
            }
        }
        #endregion

        #region ** Firebase Storage **
        // Helper: normalize bucket/path joining
        private static string CombineStoragePath(string bucket, string storagePath)
        {
            if (string.IsNullOrEmpty(bucket)) return storagePath ?? "";
            if (string.IsNullOrEmpty(storagePath)) return bucket;

            bucket = bucket.TrimEnd('/');
            storagePath = storagePath.TrimStart('/');
            return $"{bucket}/{storagePath}";
        }

        // Ensure we always have a valid CancellationTokenSource
        private CancellationToken GetCancellationToken()
        {
            if (cancellationTokenSource == null)
                cancellationTokenSource = new CancellationTokenSource();
            return cancellationTokenSource.Token;
        }

        // Retrieve a storage reference from the user specified path.
        public StorageReference GetStorageReference(string storagepath)
        {
            string storageLocation = storagepath;

            if (!(storageLocation.StartsWith("gs://", StringComparison.OrdinalIgnoreCase) ||
                  storageLocation.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                  storageLocation.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                storageLocation = CombineStoragePath(MyStorageBucket, storagepath);
            }

            if (storageLocation.StartsWith("gs://") ||
                storageLocation.StartsWith("http://") ||
                storageLocation.StartsWith("https://"))
            {
                var storageUri = new Uri(storageLocation);
                var firebaseStorage = FirebaseStorage.GetInstance($"{storageUri.Scheme}://{storageUri.Host}");
                return firebaseStorage.GetReferenceFromUrl(storageLocation);
            }

            // Fallback to default instance (shouldn't happen with above)
            return FirebaseStorage.DefaultInstance.GetReference(storageLocation);
        }
        #endregion

        #region Wait For Task Completion
        class WaitForTaskCompletion : CustomYieldInstruction
        {
            Task task;
            Firebase_Manager firebaseManager;

            public WaitForTaskCompletion(Firebase_Manager FirebaseManager, Task task)
            {
                FirebaseManager.previousTask = task;
                FirebaseManager.operationInProgress = true;
                this.firebaseManager = FirebaseManager;
                this.task = task;
            }

            public override bool keepWaiting
            {
                get
                {
                    if (task.IsCompleted)
                    {
                        firebaseManager.operationInProgress = false;
                        firebaseManager.cancellationTokenSource = new CancellationTokenSource();
                        if (task.IsFaulted)
                        {
                            firebaseManager.DisplayStorageException(task.Exception);
                        }
                        return false;
                    }
                    return true;
                }
            }
        }
        #endregion

        #region Path Helpers
        public virtual string PathToPersistentDataPathString(string filepath)
        {
            if (filepath.StartsWith(UriFileScheme, StringComparison.OrdinalIgnoreCase))
                return filepath;

            return $"{UriFileScheme}{persistentDataPath}/{filepath}";
        }
        #endregion

        #region Cancel Operation
        public void CancelOperation()
        {
            if (operationInProgress && cancellationTokenSource != null)
            {
                DebugLog("*** Cancelling operation *** ...");
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }
        #endregion

        #region Display Storage Exception
        public void DisplayStorageException(Exception exception)
        {
            var storageException = exception as StorageException;
            if (storageException != null)
            {
                DebugLog($"Error Code: {storageException.ErrorCode}");
                DebugLog($"HTTP Result Code: {storageException.HttpResultCode}");
                DebugLog($"Recoverable: {storageException.IsRecoverableException}");
                DebugLog(storageException.ToString());
            }
            else
            {
                DebugLog(exception.ToString());
            }
        }
        #endregion

        #region Display Upload Complete
        public void DisplayUploadComplete(Task<StorageMetadata> task)
        {
            if (!(task.IsFaulted || task.IsCanceled))
            {
                downloadData = new byte[0];
                fileMetadata = "";
                DebugLog("Finished uploading");
                DebugLog(MetadataToString(task.Result, false));
                DebugLog("Press the Download button to download text from Cloud Storage\n");
            }
        }
        #endregion

        #region Display Upload State
        public virtual void DisplayUploadState(UploadState uploadState)
        {
            if (operationInProgress)
            {
                DebugLog($"Uploading {uploadState.Reference.Name}: {uploadState.BytesTransferred} out of {uploadState.TotalByteCount}");
            }
        }
        #endregion

        #region Upload APIs
        public IEnumerator UploadBytes(string data, string storagepath)
        {
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Uploading to {storageReference.Path} ...");
            var token = GetCancellationToken();

            var task = storageReference.PutBytesAsync(
                Encoding.UTF8.GetBytes(data),
                StringToMetadataChange(fileMetadata),
                new StorageProgress<UploadState>(DisplayUploadState),
                token, null);

            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }

        public IEnumerator UploadStream(string data, string storagepath)
        {
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Uploading to {storageReference.Path} using stream...");
            var token = GetCancellationToken();

            var task = storageReference.PutStreamAsync(
                new MemoryStream(Encoding.ASCII.GetBytes(data)),
                StringToMetadataChange(fileMetadata),
                new StorageProgress<UploadState>(DisplayUploadState),
                token, null);

            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }

        public IEnumerator UploadFromFile(string filepath, string storagepath)
        {
            var localFilepathUriString = PathToPersistentDataPathString(filepath);
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Uploading '{localFilepathUriString}' to '{storageReference.Path}'...");
            var token = GetCancellationToken();

            var task = storageReference.PutFileAsync(
                localFilepathUriString,
                StringToMetadataChange(fileMetadata),
                new StorageProgress<UploadState>(DisplayUploadState),
                token, null);

            yield return new WaitForTaskCompletion(this, task);
            DisplayUploadComplete(task);
        }
        #endregion

        #region Update Metadata
        public IEnumerator UpdateMetadata(string metadata, string storagepath)
        {
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Updating metadata of {storageReference.Path} ...");
            var task = storageReference.UpdateMetadataAsync(StringToMetadataChange(metadata));
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                DebugLog("Updated metadata");
                DebugLog(MetadataToString(task.Result, false) + "\n");
            }
        }
        #endregion

        #region Download APIs
        public virtual void DisplayDownloadState(DownloadState downloadState)
        {
            if (operationInProgress)
            {
                DebugLog($"Downloading {downloadState.Reference.Name}: {downloadState.BytesTransferred} out of {downloadState.TotalByteCount}");
            }
        }

        public IEnumerator DownloadBytes(string storagepath, Action<byte[]> onDownloadComplete)
        {
            downloadData = new byte[0];
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Downloading {storageReference.Path} ...");
            var token = GetCancellationToken();

            // FIX: use long.MaxValue instead of 0
            var task = storageReference.GetBytesAsync(
                long.MaxValue,
                new StorageProgress<DownloadState>(DisplayDownloadState),
                token);

            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                DebugLog("Finished downloading bytes");
                downloadData = task.Result;
                DebugLog($"File Size {downloadData.Length} bytes\n");
                onDownloadComplete?.Invoke(downloadData);
            }
        }

        public async Task<byte[]> Async_DownloadBytes(string storagePath)
        {
            try
            {
                var storageReference = GetStorageReference(storagePath);
                DebugLog($"Downloading {storageReference.Path} ...");

                var data = await storageReference.GetBytesAsync(
                    long.MaxValue,
                    new StorageProgress<DownloadState>(DisplayDownloadState),
                    GetCancellationToken());

                DebugLog("Finished downloading bytes");
                DebugLog($"File Size {data.Length} bytes\n");
                return data;
            }
            catch (Exception ex)
            {
                DebugLog($"Error downloading file: {ex.Message}");
                return null;
            }
        }

        public IEnumerator DownloadStream(string storagepath)
        {
            downloadData = new byte[0];
            using (var ms = new MemoryStream())
            {
                var storageReference = GetStorageReference(storagepath);
                DebugLog($"Downloading {storageReference.Path} with stream ...");
                var token = GetCancellationToken();

                var task = storageReference.GetStreamAsync((stream) =>
                {
                    var buffer = new byte[1024];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                },
                new StorageProgress<DownloadState>(DisplayDownloadState),
                token);

                yield return new WaitForTaskCompletion(this, task);
                if (!(task.IsFaulted || task.IsCanceled))
                {
                    downloadData = ms.ToArray();
                    DebugLog("Finished downloading stream\n");
                }
            }
        }

        public string FileUriStringToPath(string fileUriString)
        {
            return Uri.UnescapeDataString((new Uri(fileUriString)).LocalPath);
        }

        public IEnumerator DownloadToFile(string storagepath, string localfilepath)
        {
            downloadData = new byte[0];
            var storageReference = GetStorageReference(storagepath);
            var localFilenameUriString = PathToPersistentDataPathString(localfilepath);
            DebugLog($"Downloading {storageReference.Path} to {localFilenameUriString}...");
            var token = GetCancellationToken();

            var task = storageReference.GetFileAsync(
              localFilenameUriString,
              new StorageProgress<DownloadState>(DisplayDownloadState),
              token);

            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                var filename = FileUriStringToPath(localFilenameUriString);
                DebugLog($"Finished downloading file {localFilenameUriString} ({filename})");
                DebugLog($"File Size {(new FileInfo(filename)).Length} bytes\n");
                downloadData = File.ReadAllBytes(filename);
            }
        }
        #endregion

        #region Delete / Metadata / URL
        public IEnumerator Delete(string storagepath)
        {
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Deleting {storageReference.Path}...");
            var task = storageReference.DeleteAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                DebugLog($"{storageReference.Path} deleted");
            }
        }

        public IEnumerator GetMetadata(string storagepath)
        {
            var storageReference = GetStorageReference(storagepath);
            DebugLog($"Bucket: {storageReference.Bucket}");
            DebugLog($"Path: {storageReference.Path}");
            DebugLog($"Name: {storageReference.Name}");
            DebugLog($"Parent Path: {(storageReference.Parent != null ? storageReference.Parent.Path : "(root)")}");
            DebugLog($"Root Path: {storageReference.Root.Path}");
            DebugLog($"App: {storageReference.Storage.App.Name}");
            var task = storageReference.GetMetadataAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
                DebugLog(MetadataToString(task.Result, false) + "\n");
        }

        public IEnumerator ShowDownloadUrl(string storagepath)
        {
            var task = GetStorageReference(storagepath).GetDownloadUrlAsync();
            yield return new WaitForTaskCompletion(this, task);
            if (!(task.IsFaulted || task.IsCanceled))
            {
                DebugLog($"DownloadUrl={task.Result}");
            }
        }
        #endregion

        #region Metadata Helpers
        MetadataChange StringToMetadataChange(string metadataString)
        {
            if (string.IsNullOrWhiteSpace(metadataString)) return null;

            var metadataChange = new MetadataChange();
            var customMetadata = new Dictionary<string, string>();
            bool hasMetadata = false;

            foreach (var metadataStringLine in metadataString.Split(new char[] { '\n' }))
            {
                if (metadataStringLine.Trim() == "")
                    continue;
                var keyValue = metadataStringLine.Split(new char[] { '=' });
                if (keyValue.Length != 2)
                {
                    DebugLog($"Ignoring malformed metadata line '{metadataStringLine}'");
                    continue;
                }
                hasMetadata = true;
                var key = keyValue[0];
                var value = keyValue[1];
                if (key == "CacheControl") metadataChange.CacheControl = value;
                else if (key == "ContentDisposition") metadataChange.ContentDisposition = value;
                else if (key == "ContentEncoding") metadataChange.ContentEncoding = value;
                else if (key == "ContentLanguage") metadataChange.ContentLanguage = value;
                else if (key == "ContentType") metadataChange.ContentType = value;
                else customMetadata[key] = value;
            }
            if (customMetadata.Count > 0)
                metadataChange.CustomMetadata = customMetadata;
            return hasMetadata ? metadataChange : null;
        }

        public string MetadataToString(StorageMetadata metadata, bool onlyMutableFields)
        {
            var fieldsAndValues = new Dictionary<string, object> {
                {"ContentType", metadata.ContentType},
                {"CacheControl", metadata.CacheControl},
                {"ContentDisposition", metadata.ContentDisposition},
                {"ContentEncoding", metadata.ContentEncoding},
                {"ContentLanguage", metadata.ContentLanguage}
            };
            if (!onlyMutableFields)
            {
                foreach (var kv in new Dictionary<string, object> {
                    {"Reference", metadata.Reference != null ? metadata.Reference.Path : null},
                    {"Path", metadata.Path},
                    {"Name", metadata.Name},
                    {"Bucket", metadata.Bucket},
                    {"Generation", metadata.Generation},
                    {"MetadataGeneration", metadata.MetadataGeneration},
                    {"CreationTimeMillis", metadata.CreationTimeMillis},
                    {"UpdatedTimeMillis", metadata.UpdatedTimeMillis},
                    {"SizeBytes", metadata.SizeBytes},
                    {"Md5Hash", metadata.Md5Hash}
                })
                {
                    fieldsAndValues[kv.Key] = kv.Value;
                }
            }
            foreach (var key in metadata.CustomMetadataKeys)
            {
                fieldsAndValues[key] = metadata.GetCustomMetadata(key);
            }
            var fieldAndValueStrings = new List<string>();
            foreach (var kv in fieldsAndValues)
            {
                fieldAndValueStrings.Add($"{kv.Key}={kv.Value}");
            }
            return string.Join("\n", fieldAndValueStrings.ToArray());
        }
        #endregion
    }

    // Small helper to silence "fire and forget" warnings intentionally
    internal static class TaskExtensions
    {
        public static void Forget(this Task task) { /* intentionally ignored */ }
    }
}
