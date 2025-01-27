(function () {
    'use strict';

    function uSyncHub($rootScope, $q, assetsService) {

        var scripts = [
            Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/lib/signalr/jquery.signalR.js',
            Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/backoffice/signalr/hubs'];

        var resource = {
            initHub: initHub
        };

        return resource;

        //////////////

        function initHub(callback) {

            if ($.connection === undefined) {
                // nothing else is using signalR (yet)
                // on this site.
                //
                // You should load it only after a check
                // because if you just initialize the 
                // scripts each time, then when something 
                // else is using signalR the settings
                // will get wiped. 

                var promises = [];
                scripts.forEach(function (script) {
                    promises.push(assetsService.loadJs(script));
                });

                // when everything is loaded setup the hub
                $q.all(promises)
                    .then(function () {
                        hubSetup(callback);
                    });
            }
            else {
                hubSetup(callback);
            }
        }

        function hubSetup(callback) {
            var proxy = $.connection.uSyncHub;

            var hub = {
                start: function () {
                    $.connection.hub.start();
                },
                on: function (eventName, callback) {
                    proxy.on(eventName, function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
                },
                invoke: function (methodName, callback) {
                    proxy.invoke(methodName)
                        .done(function (result) {
                            $rootScope.$apply(function () {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                }
            };

            return callback(hub);
        }
    }

    angular.module('umbraco.resources')
        .factory('uSyncHub', uSyncHub);
})();