angular.module('gastmanager.controllers', [])


.controller('AppCtrl', function($scope) {
})


.controller('GastCtrl', function($scope, $rootScope, SignalR, GastUpdates) {

  $scope.aktualisieren = function(gast) {
    SignalR.emit("gastmanager.app", "couchtisch;" + gast.id + ";" + gast.name + ";" + gast.farbe);
  };

  $scope.customColors = {
    "rot" : "#ff0000",
    "grün" : "#008000",
    "blau" : "#0000ff",
    "lime" : "#00ff00",
    "magenta" : "#ff1493",
    "grau" : "#a9a9a9"
  };

  $scope.gaeste = [
    {
      id: 0,
      name: GastUpdates.fuerId(0).name,
      farbe: GastUpdates.fuerId(0).farbe
    },
    {
      id: 1,
      name: GastUpdates.fuerId(1).name,
      farbe: GastUpdates.fuerId(1).farbe
    },
    {
      id: 2,
      name: GastUpdates.fuerId(2).name,
      farbe: GastUpdates.fuerId(2).farbe
    },
    {
      id: 3,
      name: GastUpdates.fuerId(3).name,
      farbe: GastUpdates.fuerId(3).farbe
    },
    {
      id: 4,
      name: GastUpdates.fuerId(4).name,
      farbe: GastUpdates.fuerId(4).farbe
    },
    {
      id: 5,
      name: GastUpdates.fuerId(5).name,
      farbe: GastUpdates.fuerId(5).farbe
    }
  ];

  $rootScope.$on("gastUpdates", function (e) {
    $scope.$apply(function () {
      $scope.gaeste = GastUpdates.alle();
    });
  });

})


.controller('EreignisseCtrl', function($scope, $rootScope, SignalR) {
  $rootScope.$on("newMessage", function (e, name, message) {
    $scope.$apply(function () {
      newMessage(name, message);
    });
  });

  var newMessage = function (name, message) {
    $scope.events.splice(0, 0, {
      timestamp: new Date(),
      clientName: name, 
      eventMessage: message
    });
  }

  $scope.events = [];
})


.controller('EinstellungenCtrl', function($scope) {
})
