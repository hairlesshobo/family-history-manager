using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Core.Operations
{
    public class GenerateRawMediaSidecarOperation
    {
        public sealed class JsonNodeConverter : IYamlTypeConverter
        {
            // Unfortunately the API does not provide those in the ReadYaml and WriteYaml
            // methods, so we are forced to set them after creation.
            public IValueSerializer ValueSerializer { get; set; }
            public IValueDeserializer ValueDeserializer { get; set; }
            
            public bool Accepts(Type type) => type == typeof(JsonObject);

            public object ReadYaml(IParser parser, Type type)
            {
                parser.Consume<MappingStart>();


                // var call = new MethodCall
                // {
                //     MethodName = (string)ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer),
                //     Arguments = (Collection<object>)ValueDeserializer.DeserializeValue(parser, typeof(Collection<object>), new SerializerState(), ValueDeserializer),
                // };

                parser.Consume<MappingEnd>();

                return "meow";
                
                // return call;
            }
            
            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                emitter.Emit(new MappingStart());

                // var call = (MethodCall)value;
                // ValueSerializer.SerializeValue(emitter, call.MethodName, typeof(string));
                // ValueSerializer.SerializeValue(emitter, call.Arguments, typeof(Collection<object>));

                emitter.Emit(new MappingEnd());
            }
        }

        public async Task StartAsync(CancellationToken cToken)
        {
            var scanner = new FileScanner(AppInfo.Config.Directories.Raw.Root);
            scanner.IncludePaths = new List<string>(AppInfo.Config.Directories.Raw.Include);
            scanner.ExcludePaths = new List<string>(AppInfo.Config.Directories.Raw.Exclude);
            scanner.Extensions = new List<string>(AppInfo.Config.Directories.Raw.Extensions);

            await foreach (var entry in scanner.StartScanAsync())
            {
                Console.WriteLine($"{entry.RelativeDepth}: {entry.Path}");

                var sidecar = await MediainfoUtils.GenerateRawSidecarAsync(entry.Path);

                // string jsonOutput = System.Text.Json.JsonSerializer.Serialize(sidecar, Static.DefaultJso);
                // Console.WriteLine(jsonOutput);

                var jsonNodeConverter = new JsonNodeConverter();

                var serializerBuilder = new SerializerBuilder()
                                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                        .WithTypeConverter(jsonNodeConverter);

                jsonNodeConverter.ValueSerializer = serializerBuilder.BuildValueSerializer();

                var serializer = serializerBuilder.Build();

                var yamlOutput = serializer.Serialize(sidecar);
                Console.WriteLine(yamlOutput);

                break;
            }
        }
    }
}