using Common.Logging;
using Splitio.Domain;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Splitio.Services.Shared.Classes
{
    public class YamlLocalhostFileService : AbstractLocalhostFileService
    {
        protected override bool IsYamlExtension => true;
        
        public YamlLocalhostFileService(ILog log)
        {
            _log = log;
        }

        public override ConcurrentDictionary<string, ParsedSplit> ParseSplitFile(string filePath)
        {
            var splits = new ConcurrentDictionary<string, ParsedSplit>();

            using (var reader = new StreamReader(File.OpenText(filePath).BaseStream))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var sequenceNodes = (YamlSequenceNode)yaml.Documents[0].RootNode;

                object keyOrKeys;
                string config;

                foreach (var children in sequenceNodes.Children)
                {
                    var mapping = ((YamlMappingNode)children).FirstOrDefault();

                    var splitName = mapping.Key.ToString();
                    var treatment = mapping.Value["treatment"]?.ToString();
                    try { keyOrKeys = mapping.Value["keys"]; } catch { keyOrKeys = null; }
                    try { config = ((YamlScalarNode)mapping.Value["config"]).Value; } catch { config = null; }

                    List<string> keys = null;
                    var splitToAdd = CreateParsedSplit(splitName, Control, new List<ConditionWithLogic>());

                    if (keyOrKeys != null)
                    {
                        keys = new List<string>();

                        if (keyOrKeys is YamlScalarNode)
                        {
                            keys.Add(((YamlScalarNode)keyOrKeys).Value);
                        }
                        else
                        {
                            var sequenceKeys = (YamlSequenceNode)keyOrKeys;
                            foreach (var aKey in sequenceKeys)
                            {
                                keys.Add(((YamlScalarNode)aKey).Value);
                            }
                        }
                    }

                    splitToAdd.conditions.Add(CreateCondition(treatment, keys));

                    if (!string.IsNullOrEmpty(config))
                    {
                        splitToAdd.configurations = new Dictionary<string, string> { { treatment, config } };
                    }

                    if (splits.ContainsKey(splitName))
                    {
                        var oldSplit = splits[splitName];

                        if (oldSplit.configurations != null)
                        {
                            splitToAdd.configurations = splitToAdd.configurations ?? new Dictionary<string, string>();

                            foreach (var conf in oldSplit.configurations)
                                splitToAdd.configurations.Add(conf.Key, conf.Value);
                        }

                        splitToAdd.conditions.AddRange(oldSplit.conditions);

                        splits.TryUpdate(splitName, splitToAdd, oldSplit);
                    }
                    else
                    {
                        splits.TryAdd(splitName, splitToAdd);
                    }
                }
            }

            return splits;
        }
    }
}
