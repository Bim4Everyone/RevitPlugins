using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using pyRevitLabs.Common;
using pyRevitLabs.Json.Linq;
using pyRevitLabs.NLog;
using pyRevitLabs.PyRevit;

namespace PlatformSettings {
    /// <summary>
    /// https://github.com/eirannejad/pyRevit/blob/7eb0a4e1f6d2509735b2da80168b1c8b0e5b2f20/dev/pyRevitLabs/pyRevitLabs.PyRevit/PyRevitExtension.cs#L35
    /// Пришлось создать дубликат класса, чтобы нормально происходила десериализация.
    /// По неизвестной мне причине во время получения boolean свойств появлялась ошибка RuntimeBinder.
    /// </summary>
    public class PyRevitExtensionDefinitionEx {
        private readonly dynamic _jsonObj;

        public PyRevitExtensionDefinitionEx(JObject jsonObj) {
            _jsonObj = jsonObj ?? throw new ArgumentNullException(nameof(jsonObj));
        }

        public PyRevitExtensionDefinitionEx(string extDefJsonFile) {
            if(string.IsNullOrEmpty(extDefJsonFile)) {
                throw new ArgumentException($"'{nameof(extDefJsonFile)}' cannot be null or empty.", nameof(extDefJsonFile));
            }

            _jsonObj = JObject.Parse(File.ReadAllText(extDefJsonFile));
            if(_jsonObj is null)
                throw new PyRevitException("jsonObj can not be null.");
        }

        public override string ToString() {
            return string.Format("Name: \"{0}\" | Type: \"{1}\" | Repo: \"{2}\"", Name, Type, Url);
        }

        public bool BuiltIn { get { return Convert.ToBoolean(_jsonObj.builtin); } }

        public bool DefaultEnabled { get { return Convert.ToBoolean(_jsonObj.default_enabled); } }

        public bool RocketModeCompatible { get { return Convert.ToBoolean(_jsonObj.rocket_mode_compatible); } }

        public string Name { get { return _jsonObj.name; } }

        public PyRevitExtensionTypes Type {
            get {
                switch("." + _jsonObj.type) {
                    case PyRevitConsts.ExtensionUIPostfix:
                    return PyRevitExtensionTypes.UIExtension;
                    case PyRevitConsts.ExtensionLibraryPostfix:
                    return PyRevitExtensionTypes.LibraryExtension;
                    default:
                    return PyRevitExtensionTypes.Unknown;
                }
            }
        }

        public string Description { get { return _jsonObj.description; } }

        public string Author { get { return _jsonObj.author; } }

        public string AuthorProfile { get { return _jsonObj.author_url; } }

        public string Url { get { return _jsonObj.url; } }

        public string Website { get { return _jsonObj.website; } }

        public string ImageUrl { get { return _jsonObj.image; } }

        public dynamic Templates { get { return _jsonObj.templates; } }

        public dynamic Dependencies { get { return _jsonObj.dependencies; } }
    }

    internal static class PyRevitExtensionsEx {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Возвращает полное наименование расширения (BIM4Everyone.lib).
        /// </summary>
        /// <param name="extension">Расширение pyRevit.</param>
        /// <returns>Возвращает полное наименование расширения (BIM4Everyone.lib).</returns>
        public static string GetExtensionName(this PyRevitExtensionDefinitionEx extension) {
            logger.Debug("Getting extension name \"{0}\"", extension.Name);
            return extension.Name + PyRevitExtension.GetExtensionDirExt(extension.Type);
        }

        /// <summary>
        /// Возвращает статус включения расширения.
        /// </summary>
        /// <param name="extName">Полное наименование расширения.</param>
        /// <returns>Возвращает статус включения расширения. <see cref="true"/> - если расширение включено, иначе <see cref="false"/>.</returns>
        public static bool IsEnabledExtension(string extName) {
            logger.Debug("Getting state extension \"{0}\"", extName);

            string disabled = PyRevitConfigs.GetConfigFile().GetValue(extName, "disabled");
            return string.IsNullOrEmpty(disabled) ? false : !bool.Parse(disabled);
        }

        /// <summary>
        /// Переключает состояние расширения.
        /// </summary>
        /// <param name="extName">Полное наименование расширения.</param>
        /// <param name="state">Состояние расширения <see cref="true"/> - расширение включено, <see cref="false"/> выключено</param>
        public static void ToggleExtension(string extName, bool state) {
            logger.Debug("{0} extension \"{1}\"", state ? "Enabling" : "Disabling", extName);
            PyRevitConfigs.GetConfigFile().SetValue(extName, "disabled", !state);
        }

        /// <summary>
        /// enable extension in config
        /// https://github.com/eirannejad/pyRevit/blob/master/dev/pyRevitLabs/pyRevitLabs.PyRevit/PyRevitExtensions.cs#L281
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="state"></param>
        public static void ToggleExtension(PyRevitExtension ext, bool state) {
            logger.Debug("{0} extension \"{1}\"", state ? "Enabling" : "Disabling", ext.Name);
            PyRevitConfigs.GetConfigFile().SetValue(ext.ConfigName, "disabled", !state);
        }

        /// <summary>
        /// Check if extension name matches the given pattern.
        /// https://github.com/eirannejad/pyRevit/blob/master/dev/pyRevitLabs/pyRevitLabs.PyRevit/PyRevitExtensions.cs#L29
        /// </summary>
        /// <param name="extName"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static bool CompareExtensionNames(string extName, string searchTerm) {
            var extMatcher = new Regex(searchTerm,
                                       RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return extMatcher.IsMatch(extName);
        }

        /// <summary>
        /// Find extension with search patten in extension lookup resource (file or url to a remote file).
        /// https://github.com/eirannejad/pyRevit/blob/master/dev/pyRevitLabs/pyRevitLabs.PyRevit/PyRevitExtensions.cs#L424
        /// </summary>
        /// <param name="fileOrUri"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static List<PyRevitExtensionDefinitionEx> LookupExtensionInDefinitionFile(
                string fileOrUri,
                string searchPattern = null) {
            var pyrevtExts = new List<PyRevitExtensionDefinitionEx>();
            string filePath = null;

            // determine if path is file or uri
            logger.Debug("Determining file or remote source \"{0}\"", fileOrUri);
            Uri uriResult;
            var validPath = Uri.TryCreate(fileOrUri, UriKind.Absolute, out uriResult);
            if(validPath) {
                if(uriResult.IsFile) {
                    filePath = fileOrUri;
                    logger.Debug("Source is a file \"{0}\"", filePath);
                } else if(uriResult.HostNameType == UriHostNameType.Dns
                              || uriResult.HostNameType == UriHostNameType.IPv4
                              || uriResult.HostNameType == UriHostNameType.IPv6) {

                    logger.Debug("Source is a remote resource \"{0}\"", fileOrUri);
                    logger.Debug("Downloading remote resource \"{0}\"...", fileOrUri);
                    // download the resource into TEMP
                    try {
                        filePath =
                            CommonUtils.DownloadFile(fileOrUri,
                                                     Path.Combine(Environment.GetEnvironmentVariable("TEMP"),
                                                                  PyRevitConsts.EnvConfigsExtensionDBFileName)
                            );
                    } catch(Exception ex) {
                        throw new PyRevitException(
                            string.Format("Error downloading extension metadata file. | {0}", ex.Message)
                            );
                    }
                }
            } else
                throw new PyRevitException(
                    string.Format("Source is not a valid file or remote resource \"{0}\"", fileOrUri)
                    );

            // process file now
            if(filePath != null) {
                if(Path.GetExtension(filePath).ToLower() == ".json") {
                    logger.Debug("Parsing extension metadata file...");

                    dynamic extensionsObj;
                    if(filePath != null) {
                        try {
                            extensionsObj = JObject.Parse(File.ReadAllText(filePath));
                        } catch(Exception ex) {
                            throw new PyRevitException(string.Format("Error parsing extension metadata. | {0}", ex.Message));
                        }

                        if(extensionsObj.extensions == null) {
                            var extDef = new PyRevitExtensionDefinitionEx((JObject) extensionsObj);
                            pyrevtExts.Add(extDef);
                            return pyrevtExts;
                        }

                        // make extension list
                        foreach(JObject extObj in extensionsObj.extensions) {
                            var extDef = new PyRevitExtensionDefinitionEx(extObj);

                            logger.Debug("Registered extension \"{0}\"", extDef.Name);
                            if(searchPattern != null) {
                                if(CompareExtensionNames(extDef.Name, searchPattern)) {
                                    logger.Debug(string.Format("\"{0}\" Matched registered extension \"{1}\"",
                                                               searchPattern, extDef.Name));
                                    pyrevtExts.Add(extDef);
                                }
                            } else
                                pyrevtExts.Add(extDef);
                        }
                    }
                } else
                    throw new PyRevitException(
                        string.Format("Definition file is not a valid json file \"{0}\"", filePath)
                        );
            }

            return pyrevtExts;
        }
    }
}
