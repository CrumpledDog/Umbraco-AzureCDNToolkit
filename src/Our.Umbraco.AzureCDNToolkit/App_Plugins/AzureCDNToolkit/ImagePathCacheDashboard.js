angular
    .module('umbraco')

        .controller('AzureCDNToolKit.CacheController', ['$scope', '$http', function ($scope, $http) {

          // Get WebApi urls
          var cacheApiBaseUrl = Umbraco.Sys.ServerVariables.azureCdnToolkitUrls.cacheApiBaseUrl;
          var sendCachedImagesRequestApiUrl = cacheApiBaseUrl + "SendCachedImagesRequest";
          var getAllCachedImagesFromRequestApiUrl = cacheApiBaseUrl + "GetAllCachedImagesFromRequest";
          var wipeApiUrl = cacheApiBaseUrl + "Wipe";
          var getAllServersApiUrl = cacheApiBaseUrl + "GetAllServers";

          $scope.statuses = [];
          $scope.waiting = false;

          $scope.getAllCachedImagesForServer = function (server) {
            $scope.statuses = [];
            $scope.success = false;
            $http({
              method: 'POST',
              url: sendCachedImagesRequestApiUrl,
              params: {
                'serverIdentity': server
              },
              data: {}
            })
            .success(function (data, status, headers, config) {
              var requestId;
              if (typeof data === "string") {
                requestId = JSON.parse(data);
              }
              else {
                requestId = data;
              }
              $scope.requestId = requestId;
            })
            .then(function () {

              var getResponse = function () {

                $scope.waiting = true;

                // do something with the request id to get the data we actually want
                $http({
                  method: 'POST',
                  url: getAllCachedImagesFromRequestApiUrl,
                  params: {
                    'requestId': $scope.requestId
                  },
                  data: {}
                }).success(function (data, status, headers, config) {

                  if (angular.isArray(data)) {
                    $scope.statuses = data;
                    $scope.success = true;
                    $scope.waiting = false;
                  } else {
                    // try again
                    getResponse();
                  }
                });
              };

              getResponse();

            });
          };

          $scope.rowClass = function (image) {
            if (image.resolved === false) {
              return "warning";
            }
            if (image.weburl === "" || image.cacheurl === "") {
              return "error";
            }
            if (image.weburl !== image.cacheurl) {
              return "success";
            }
            return "warning";
          }

          $scope.wipe = function (server, weburl) {
            $http({
              method: 'POST',
              url: wipeApiUrl,
              params: {
                'serverIdentity': server,
                'weburl': weburl
              },
              data: {}
            }).success(function () {
              // clear all triggered
              if (typeof (weburl) == "undefined") {
                $scope.statuses = [];
              } else {
                // remove from view
                $scope.statuses = $scope.statuses.filter(function (value) { return value.weburl !== weburl; });
              }
            });
          };

          $scope.getAllServers = function () {

            var getResponse = function() {
              $http.get(getAllServersApiUrl)
                .success(function (data) {
                  if (angular.isArray(data)) {
                    $scope.servers = data;
                    $scope.selectedserver = $scope.servers[0];
                  } else {
                    // try again
                    getResponse();
                  }
                });
            };

            getResponse();

          };

          $scope.getAllServers();

        }]);