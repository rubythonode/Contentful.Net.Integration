﻿using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Net.CMA;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Contentful.Net.Integration
{
    [TestCaseOrderer("Contentful.Net.CMA.CustomTestCaseOrderer", "Contentful.Net.CMA")]
    public class ContentfulCMATests : IClassFixture<SpaceFixture>
    {
        private ContentfulManagementClient _client;
        private string _spaceId = "";
        private string _contentTypeId = "contenttype";

        public ContentfulCMATests(SpaceFixture fixture)
        {

            var httpClient = new HttpClient(new TestEndpointMessageHandler());
            var managementToken = Environment.GetEnvironmentVariable("CONTENTFUL_ACCESS_TOKEN");
            _spaceId = fixture.SpaceId;
            _client = new ContentfulManagementClient(httpClient, new ContentfulOptions()
            {
                DeliveryApiKey = "123",
                ManagementApiKey = managementToken,
                SpaceId = fixture.SpaceId,
                UsePreviewApi = false
            });

        }

        [Fact]
        [Order(5)]
        public async Task GetASpace()
        {
            var space = await _client.GetSpaceAsync(_spaceId);

            Assert.Equal("dotnet-test-space", space.Name);
        }

        [Fact]
        [Order(10)]
        public async Task GetAllSpaces()
        {
            var spaces = await _client.GetSpacesAsync();

            Assert.Contains(spaces, c => c.Name == "dotnet-test-space");
        }

        [Fact]
        [Order(20)]
        public async Task UpdateSpaceName()
        {
            var space = await _client.UpdateSpaceNameAsync(_spaceId, "knuckleburger", 1);
            Assert.Equal("knuckleburger", space.Name);
            space.Name = "dotnet-test-space";
            space = await _client.UpdateSpaceNameAsync(space);
            Assert.Equal("dotnet-test-space", space.Name);
        }

        [Fact]
        [Order(25)]
        public async Task CreateContentType()
        {
            var contentType = new ContentType();
            contentType.SystemProperties = new SystemProperties()
            {
                Id = _contentTypeId
            };
            contentType.Name = "Cool content";
            contentType.Fields = new List<Field>()
            {
                new Field()
                {
                    Name = "Field1",
                    Id = "field1",
                    @Type = "Text"
                },
                new Field()
                {
                    Name = "Field2",
                    Id = "field2",
                    @Type = "Text"
                }
            };

            var contenttype = await _client.CreateOrUpdateContentTypeAsync(contentType, _spaceId);

            Assert.Equal(2, contenttype.Fields.Count);
        }

        [Fact]
        [Order(30)]
        public async Task UpdateContentType()
        {
            var contentType = new ContentType();
            contentType.SystemProperties = new SystemProperties()
            {
                Id = _contentTypeId
            };
            contentType.Name = "Cool content changed";
            contentType.Fields = new List<Field>()
            {
                new Field()
                {
                    Name = "Field1",
                    Id = "field1",
                    @Type = "Text"
                },
                new Field()
                {
                    Name = "Field2",
                    Id = "field2",
                    @Type = "Text"
                }
            };

            var updatedContentType = await _client.CreateOrUpdateContentTypeAsync(contentType, _spaceId, version: 1);

            Assert.Equal(2, updatedContentType.Fields.Count);
            Assert.Equal("Cool content changed", updatedContentType.Name);
        }

        [Fact]
        [Order(40)]
        public async Task GetContentType()
        {
            var contentType = await _client.GetContentTypeAsync(_contentTypeId, _spaceId);

            Assert.Equal(_contentTypeId, contentType.SystemProperties.Id);
        }

        [Fact]
        [Order(50)]
        public async Task GetAllContentTypes()
        {
            //It seems we need to give the API a chance to catch up...
            Thread.Sleep(5000);
            var contentTypes = await _client.GetContentTypesAsync();

            Assert.Equal(1, contentTypes.Count());
        }

        [Fact]
        [Order(60)]
        public async Task PublishContentType()
        {
            var contentType = await _client.ActivateContentTypeAsync(_contentTypeId, 2);

            Assert.Equal(3, contentType.SystemProperties.Version);
        }

        [Fact]
        [Order(80)]
        public async Task CreateEntry()
        {
            var entry = new Entry<dynamic>();

            entry.SystemProperties = new SystemProperties()
            {
                Id = "entry1"
            };

            entry.Fields = new JObject
            (
                new JProperty("field1", new JObject(new JProperty("en-US", "bla"))),
                new JProperty("field2", new JObject(new JProperty("en-US", "blue")))
            );

            entry = await _client.CreateOrUpdateEntryAsync(entry, contentTypeId: _contentTypeId);

            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(90)]
        public async Task GetAllEntries()
        {
            //It seems we need to give the API a chance to catch up...
            Thread.Sleep(5000);
            var entries = await _client.GetEntriesCollectionAsync<Entry<dynamic>>();

            Assert.Equal(1, entries.Count());
        }

        [Fact]
        [Order(100)]
        public async Task GetEntry()
        {
            var entry = await _client.GetEntryAsync("entry1");

            Assert.Equal(1, entry.SystemProperties.Version);
            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(110)]
        public async Task PublishEntry()
        {
            var entry = await _client.PublishEntryAsync("entry1", 1);

            Assert.Equal(2, entry.SystemProperties.Version);
            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(120)]
        public async Task UnpublishEntry()
        {
            var entry = await _client.UnpublishEntryAsync("entry1", 2);

            Assert.Equal(3, entry.SystemProperties.Version);
            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(130)]
        public async Task ArchiveEntry()
        {
            var entry = await _client.ArchiveEntryAsync("entry1", 3);

            Assert.Equal(4, entry.SystemProperties.Version);
            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(140)]
        public async Task UnarchiveEntry()
        {
            var entry = await _client.UnarchiveEntryAsync("entry1", 4);

            Assert.Equal(5, entry.SystemProperties.Version);
            Assert.Equal("bla", entry.Fields.field1["en-US"].ToString());
        }

        [Fact]
        [Order(145)]
        public async Task GetAssets()
        {
            var assets = await _client.GetAssetsCollectionAsync();

            Assert.Empty(assets);

            var publishedAssets = await _client.GetPublishedAssetsCollectionAsync();

            Assert.Empty(publishedAssets);
        }

        [Fact]
        [Order(150)]
        public async Task CreateAsset()
        {
            var asset = new ManagementAsset();
            asset.SystemProperties = new SystemProperties()
            {
                Id = "asset1"
            };
            asset.Title = new Dictionary<string, string>()
            {
                { "en-US", "AssetMaster" }
            };
            asset.Files = new Dictionary<string, File>
            {
                { "en-US", new File()
                    {
                        ContentType ="image/jpeg",
                        FileName = "moby.png",
                        UploadUrl = "https://robertlinde.se/assets/top/moby-top.png"
                    }
                }
            };

            var createdAsset = await _client.CreateOrUpdateAssetAsync(asset);

            Assert.Equal("AssetMaster", createdAsset.Title["en-US"]);
        }

        [Fact]
        [Order(160)]
        public async Task GetAsset()
        {
            var asset = await _client.GetAssetAsync("asset1");

            Assert.Equal("AssetMaster", asset.Title["en-US"]);
        }

        [Fact]
        [Order(170)]
        public async Task ProcessAsset()
        {
            await _client.ProcessAssetAsync("asset1",1 , "en-US");

            Assert.True(true);
        }

        [Fact]
        [Order(180)]
        public async Task PublishAsset()
        {
            //Give processing a chance to finish...
            Thread.Sleep(5000);
            ManagementAsset asset = null;

            asset = await _client.PublishAssetAsync("asset1", 2);

            Assert.Equal("AssetMaster", asset.Title["en-US"]);
        }

        [Fact]
        [Order(190)]
        public async Task UnpublishAsset()
        {
            var asset = await _client.UnpublishAssetAsync("asset1", 3);

            Assert.Equal("AssetMaster", asset.Title["en-US"]);
        }

        [Fact]
        [Order(200)]
        public async Task ArchiveAsset()
        {
            var asset = await _client.ArchiveAssetAsync("asset1", 4);

            Assert.Equal("AssetMaster", asset.Title["en-US"]);
        }

        [Fact]
        [Order(210)]
        public async Task UnarchiveAsset()
        {
            var asset = await _client.UnarchiveAssetAsync("asset1", 5);

            Assert.Equal("AssetMaster", asset.Title["en-US"]);
        }

        

        [Fact]
        [Order(220)]
        public async Task GetLocales()
        {
            var locales = await _client.GetLocalesCollectionAsync();

            Assert.Equal(1, locales.Total);
        }

        [Fact]
        [Order(230)]
        public async Task CreateGetUpdateDeleteLocale()
        {
            var locale = new Locale();
            locale.Code = "c-sharp";
            locale.Name = "See Sharp";
            locale.FallbackCode = "en-US";
            locale.Optional = true;
            locale.ContentDeliveryApi = true;
            locale.ContentManagementApi = true;

            var createdLocale = await _client.CreateLocaleAsync(locale);

            Assert.Equal("c-sharp", createdLocale.Code);

            locale = await _client.GetLocaleAsync(createdLocale.SystemProperties.Id);

            Assert.Equal("See Sharp", locale.Name);

            locale.Name = "c#";

            locale = await _client.UpdateLocaleAsync(locale);

            Assert.Equal("c#", locale.Name);

            await _client.DeleteLocaleAsync(locale.SystemProperties.Id);
        }

        [Fact]
        [Order(240)]
        public async Task GetAllWebHooks()
        {
            var hooks = await _client.GetWebHooksCollectionAsync();

            Assert.Equal(0, hooks.Total);
        }

        [Fact]
        [Order(250)]
        public async Task CreateGetUpdateDeleteWebHook()
        {
            var webHook = new WebHook();
            webHook.Name = "Captain Hook";
            webHook.Url = "https://robertlinde.se";
            webHook.HttpBasicPassword = "Pass";
            webHook.HttpBasicUsername = "User";
            webHook.Headers = new List<KeyValuePair<string, string>>();
            webHook.Headers.Add(new KeyValuePair<string, string>("ben", "long"));
            webHook.Topics = new List<string>()
            {
                "Entry.*"
            };

            var createdHook = await _client.CreateWebHookAsync(webHook);

            Assert.Equal("Captain Hook", createdHook.Name);

            webHook.Name = "Dustin Hoffman";
            webHook.SystemProperties = new SystemProperties()
            {
                Id = createdHook.SystemProperties.Id
            };

            var updatedHook = await _client.CreateOrUpdateWebHookAsync(webHook);

            Assert.Equal("Dustin Hoffman", updatedHook.Name);

            webHook = await _client.GetWebHookAsync(updatedHook.SystemProperties.Id);

            Assert.Equal("Dustin Hoffman", webHook.Name);

            await _client.DeleteWebHookAsync(updatedHook.SystemProperties.Id);
        }

        [Fact]
        [Order(610)]
        public async Task GetApiKeys()
        {
            var keys = await _client.GetAllApiKeysAsync();

            Assert.Equal(0, keys.Total);
        }

        [Fact]
        [Order(620)]
        public async Task CreateApiKey()
        {
            var createdKey = await _client.CreateApiKeyAsync("Keyport", "blahblah");

            Assert.Equal("Keyport", createdKey.Name);
            Assert.NotNull(createdKey.AccessToken);
        }

        [Fact]
        [Order(600)]
        public async Task DeleteEntry()
        {
            await _client.DeleteEntryAsync("entry1", 1);
        }

        [Fact]
        [Order(610)]
        public async Task DeleteAsset()
        {
            await _client.DeleteAssetAsync("asset1", 5);
        }

        [Fact]
        [Order(700)]
        public async Task UnpublishContentType()
        {
            //It seems we need to give the API a chance to catch up...
            Thread.Sleep(2000);
            var contentTypes = await _client.GetActivatedContentTypesAsync();

            Assert.Equal(1, contentTypes.Count());
            await _client.DeactivateContentTypeAsync(_contentTypeId);

            contentTypes = await _client.GetActivatedContentTypesAsync();
            Assert.Empty(contentTypes);
        }

        [Fact]
        [Order(720)]
        public async Task DeleteContentType()
        {
            await _client.DeleteContentTypeAsync(_contentTypeId);
        }

        [Fact]
        [Order(1000)]
        public async Task DeleteSpace()
        {
            await _client.DeleteSpaceAsync(_spaceId);
        }

    }

    public class SpaceFixture : IDisposable
    {
        public string SpaceId { get; set; }
        private readonly ContentfulManagementClient _client;
        public SpaceFixture()
        {
            var httpClient = new HttpClient(new TestEndpointMessageHandler());
            var managementToken = Environment.GetEnvironmentVariable("CONTENTFUL_ACCESS_TOKEN");
            _client = new ContentfulManagementClient(httpClient, new ContentfulOptions()
            {
                DeliveryApiKey = "123",
                ManagementApiKey = managementToken,
                SpaceId = "123",
                UsePreviewApi = false
            });

            var space = _client.CreateSpaceAsync("dotnet-test-space", "en-US", organisation: "0w9KPTOuMqrBnj9GpkDAwF").Result;

            SpaceId = space.SystemProperties.Id;
        }

        public void Dispose()
        {
            try
            {
                _client.DeleteSpaceAsync(SpaceId);
            }
            catch(Exception)
            {
                //We don't really wanna do anything here, 
                //just try to delete the space if it's left over for some reason
            }
        }
    }

    public class TestEndpointMessageHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Environment.GetEnvironmentVariable("CONTENTFUL_RUN_WITHOUT_PROXY") == "true")
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var requestUrl = request.RequestUri.ToString();

            requestUrl = requestUrl
                .Replace("https://api.contentful.com/", "http://127.0.0.1:5000/");

            request.RequestUri = new Uri(requestUrl);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
