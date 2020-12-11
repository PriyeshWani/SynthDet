using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Simulation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using System.IO;
using UnityEditor;
using Object = System.Object;

public class ProjectInitialization : MonoBehaviour
{
    static readonly Guid k_AppParamsMetricGuid = new Guid("3F06BCEC-1F23-4387-A1FD-5AF54EE29C16");
    
    // Defaults are shared between the ProjectInitialization inspector GUI and the USim execution window, so they
    // defined one here statically to be used in both places
    public static readonly AppParams AppParamDefaults = new AppParams()
    {
        ScaleFactors = new[] { 1.0f, .5f},
        MaxFrames = 5000,
        MaxForegroundObjectsPerFrame = 500,
        NumBackgroundFillPasses = 1,
        BackgroundObjectDensity = 3,
        ScalingMin = 0.2f,
        ScalingSize = 0.1f,
        LightColorMin = 0.1f,
        LightRotationMax = 90f,
        BackgroundHueMaxOffset = 180,
        OccludingHueMaxOffset = 180f,
        BackgroundObjectInForegroundChance = .2f,
        NoiseStrengthMax = 0.02f,
        BlurKernelSizeMax = 0.01f,
        BlurStandardDeviationMax = 0.5f
    };
    public string BackgroundObjectResourcesDirectory = "Background";
    public string BackgroundImageResourcesDirectory = "GroceryStoreDataset";

    public AppParams AppParameters = AppParamDefaults;
    public bool EnableProfileLog;
    public PerceptionCamera PerceptionCamera;
    public IdLabelConfig idLabelconfig;
    public GameObject[] foregroundObjects;
    Entity m_ResourceDirectoriesEntity;
    Entity m_CurriculumStateEntity;
    string m_ProfileLogPath;
    PlacementStatics m_PlacementStatics;
    private Dictionary<string, GameObject> m_ResourcesDirectory;
    private Dictionary<string, Texture2D> m_ImagesDictionary;

    void Start()
    {
        var backgroundObjects = Resources.LoadAll<GameObject>(BackgroundObjectResourcesDirectory);
        var backgroundImages = Resources.LoadAll<Texture2D>(BackgroundImageResourcesDirectory);

        if (foregroundObjects.Length == 0)
        {
            Debug.LogError($"No Prefabs given in Foreground Objects list.");
            return;
        }
        if (backgroundObjects.Length == 0)
        {
            Debug.LogError($"No Prefabs of FBX files found in background object directory \"{BackgroundObjectResourcesDirectory}\".");
            return;
        }
        //TODO: Fill in CurriculumState from app params
        if (TryGetAppParamPathFromCommandLine(out string appParamPath))
        {
            var AppParamsJson = File.ReadAllText(appParamPath);
            AppParameters = JsonUtility.FromJson<AppParams>(AppParamsJson);
        }
        else if (!String.IsNullOrEmpty(Configuration.Instance.SimulationConfig.app_param_uri))
        {
            AppParameters = Configuration.Instance.GetAppParams<AppParams>();
        }
        
        Debug.Log($"{nameof(ProjectInitialization)}: Starting up. MaxFrames: {AppParameters.MaxFrames}, " +
            $"scale factors {{{string.Join(", ", AppParameters.ScaleFactors)}}}");

        var basePath = Path.Combine(Application.dataPath, "Resources");
        //PopulateResourcesPrefabsDirecotory<GameObject>(Path.Combine(basePath, BackgroundObjectResourcesDirectory), ref m_ResourcesDirectory, new []{ ".fbx"});
        //PopulateResourcesPrefabsDirecotory<Texture2D>(Path.Combine(basePath,BackgroundImageResourcesDirectory), ref m_ImagesDictionary, new [] {".jpg", ".png"});
        PopulateResources();
        
        m_PlacementStatics = new PlacementStatics(
            AppParameters.MaxFrames,
            AppParameters.MaxForegroundObjectsPerFrame,
            AppParameters.ScalingMin,
            AppParameters.ScalingSize,
            AppParameters.OccludingHueMaxOffset,
            AppParameters.BackgroundObjectInForegroundChance,
            foregroundObjects,
            m_ResourcesDirectory,
            m_ImagesDictionary,
            ObjectPlacementUtilities.GenerateInPlaneRotationCurriculum(Allocator.Persistent),
            ObjectPlacementUtilities.GenerateOutOfPlaneRotationCurriculum(Allocator.Persistent),
            new NativeArray<float>(AppParameters.ScaleFactors, Allocator.Persistent),
            idLabelconfig);
        var appParamsMetricDefinition = DatasetCapture.RegisterMetricDefinition(
            "app-params", description:"The values from the app-params used in the simulation. Only triggered once per simulation.", id: k_AppParamsMetricGuid);
        DatasetCapture.ReportMetric(appParamsMetricDefinition, new[] {AppParameters});
        m_CurriculumStateEntity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(
            m_CurriculumStateEntity, new CurriculumState());
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentObject(
            m_CurriculumStateEntity, m_PlacementStatics);

        ValidateForegroundLabeling(foregroundObjects, PerceptionCamera);
        
#if !UNITY_EDITOR
        if (Debug.isDebugBuild && EnableProfileLog)
        {
            Debug.Log($"Enabling profile capture");
            m_ProfileLogPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "profileLog.raw");
            if (System.IO.File.Exists(m_ProfileLogPath))
                System.IO.File.Delete(m_ProfileLogPath);

            UnityEngine.Profiling.Profiler.logFile = m_ProfileLogPath;
            UnityEngine.Profiling.Profiler.enabled = true;
            UnityEngine.Profiling.Profiler.enableBinaryLog = true;

        }
#endif
        Manager.Instance.ShutdownNotification += CleanupState;
        
        //PerceptionCamera.renderedObjectInfosCalculated += OnRenderedObjectInfosCalculated;
    }


    private void PopulateResources()
    {
        var resPaths = Resources.Load<ResourcesPaths>("ResourcesRelativePaths");
        Debug.Assert(resPaths != null, "ResPaths is null");

        var imagesRelPaths = Resources.Load<ImagesResourcesPath>("ImageResourcesRelativePaths");
        Debug.Assert(imagesRelPaths != null, "Images Resources Relative paths is null");

        foreach (var prefab in resPaths.prefabs)
        {
            if (m_ResourcesDirectory == null)
                m_ResourcesDirectory = new Dictionary<string, GameObject>();
            
            m_ResourcesDirectory.Add(prefab.relativePath, prefab.gameObject);
        }
        
        foreach (var texture in imagesRelPaths.textures)
        {
            if (m_ImagesDictionary == null)
                m_ImagesDictionary = new Dictionary<string, Texture2D>();
            
            m_ImagesDictionary.Add(texture.relativePath, texture.texture);
        }
    }
    
    
    #if UNITY_EDITOR
    [MenuItem("Tools/ResourcesPaths")]
    public static void PopulateResourcesPathAsset()
    {
        var resPaths = ScriptableObject.CreateInstance<ResourcesPaths>();
        var imagesPaths = ScriptableObject.CreateInstance<ImagesResourcesPath>();
        var go = GameObject.FindObjectOfType<ProjectInitialization>();
        var basePath = Path.Combine(Application.dataPath, "Resources");
        if (go != null)
        {
            go.PopulateResourcesPrefabsDirecotory(Path.Combine(basePath, go.BackgroundObjectResourcesDirectory), ref go.m_ResourcesDirectory, new []{ ".fbx"});
            go.PopulateResourcesPrefabsDirecotory(Path.Combine(basePath, go.BackgroundImageResourcesDirectory), ref go.m_ImagesDictionary, new [] {".jpg", ".png"});
        }

        foreach (var entry in go.m_ResourcesDirectory)
        {
            if (resPaths.prefabs == null)
            {
                resPaths.prefabs = new List<Prefab>();
            }
            
            resPaths.prefabs.Add(new Prefab()
            {
                relativePath = entry.Key,
                gameObject = entry.Value
            });
        }
        
        foreach (var entry in go.m_ImagesDictionary)
        {
            if (imagesPaths.textures == null)
            {
                imagesPaths.textures = new List<TextureResource>();
            }
            
            imagesPaths.textures.Add(new TextureResource()
            {
                relativePath = entry.Key,
                texture = entry.Value
            });
        }
        
        AssetDatabase.CreateAsset(resPaths, "Assets/ResourcesRelativePaths.asset");
        AssetDatabase.CreateAsset(imagesPaths, "Assets/ImageResourcesRelativePaths.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = resPaths;

    }
    #endif

    private void PopulateResourcesPrefabsDirecotory<T>(string basePath, ref Dictionary<string, T> dir, 
        string[] assetExtensions) where T : UnityEngine.Object
    {
        
        var directories = Directory.GetDirectories(basePath);

        foreach (var directory in directories)
        {
            PopulateResourcesPrefabsDirecotory<T>(directory, ref dir, assetExtensions);
        }

        if (dir == null)
            dir = new Dictionary<string, T>();

        var path = Path.Combine(Application.dataPath, basePath);
        if (!Directory.Exists(path))
            Log.E("No directory named " + basePath + " is found");

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (assetExtensions.Contains(extension))
            {
                var fileRelativePath = file.Split(new string[] {"/Resources/"}, StringSplitOptions.None)[1];
                string filePathWithoutExt = fileRelativePath.Substring(0, fileRelativePath.Length - extension.Length);
                var prefab = Resources.Load(filePathWithoutExt, typeof(T)) as T;
                dir.Add(filePathWithoutExt, prefab);   
            }
        }
    }

    static bool TryGetAppParamPathFromCommandLine(out string appParamPath)
    {
        appParamPath = null;
        var appParamArg = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--app-param"));
        if (appParamArg == null)
            return false;

        var strings = appParamArg.Split('=');
        if (strings.Length < 2)
            return false;

        appParamPath = strings[1].Trim('"');
        return true;
    }

    void OnRenderedObjectInfosCalculated(int frameCount, NativeArray<RenderedObjectInfo> renderedObjectinfos)
    {
        foreach (var info in renderedObjectinfos)
        {
            if (info.pixelCount < 50)
            {
                Debug.Log($"Found small bounding box {info} in frame {frameCount}");
            }
        }
    }

    void ValidateForegroundLabeling(GameObject[] foregroundObjects, PerceptionCamera perceptionCamera)
    {
        
        var boundingBox2DLabeler = (BoundingBox2DLabeler)perceptionCamera.labelers.First(l => l is BoundingBox2DLabeler);
        if (boundingBox2DLabeler == null)
            return;
        var labelConfig = boundingBox2DLabeler.idLabelConfig;
        if (labelConfig == null)
        {
            Debug.LogError("PerceptionCamera does not have a labeling configuration. This will likely cause the program to fail.");
            return;
        }

        var foregroundObjectsMissingFromConfig = new List<GameObject>();
        var foundLabels = new List<string>();
        foreach (var foregroundObject in foregroundObjects)
        {
            var labeling = foregroundObject.GetComponent<Labeling>();
            if (labeling == null)
            {
                foregroundObjectsMissingFromConfig.Add(foregroundObject);
                continue;
            }

            bool found = false;
            foreach (var label in labeling.labels)
            {
                if (labelConfig.labelEntries.Select(e => e.label).Contains(label))
                {
                    foundLabels.Add(label);
                    found = true;
                    break;
                }
            }

            if (!found)
                foregroundObjectsMissingFromConfig.Add(foregroundObject);
        }

        if (foregroundObjectsMissingFromConfig.Count > 0)
        {
            Debug.LogError($"The following foreground models are not present in the LabelingConfiguration: {string.Join(", ", foregroundObjectsMissingFromConfig)}");
        }
        
        var configurationsMissingModel = labelConfig.labelEntries.Select(l => l.label).Where(l => !foundLabels.Contains(l)).ToArray();
        if (configurationsMissingModel.Length > 0)
        {
            Debug.LogError($"The following LabelingConfiguration entries do not correspond to any foreground object model: {string.Join(", ", configurationsMissingModel)}");
        }
    }

    void CleanupState()
    {
#if !UNITY_EDITOR
        if (Debug.isDebugBuild && EnableProfileLog)
        {
            Debug.Log($"Producing profile capture.");
            UnityEngine.Profiling.Profiler.enabled = false;
            var targetPath = Path.Combine(Manager.Instance.GetDirectoryFor("Profiling"), "profileLog.raw");
            File.Copy(m_ProfileLogPath, targetPath);
            Manager.Instance.ConsumerFileProduced(targetPath);
        }
#endif

        m_PlacementStatics.ScaleFactors.Dispose();
        m_PlacementStatics.InPlaneRotations.Dispose();
        m_PlacementStatics.OutOfPlaneRotations.Dispose();
        World.DefaultGameObjectInjectionWorld?.EntityManager?.DestroyEntity(m_ResourceDirectoriesEntity);
        World.DefaultGameObjectInjectionWorld?.EntityManager?.DestroyEntity(m_CurriculumStateEntity);
    }
}
