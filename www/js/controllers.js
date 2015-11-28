angular.module('gastmanager.controllers', [])

.controller('AppCtrl', function($scope, $ionicModal, $timeout) {

})

.controller('GastCtrl', function($scope, SignalR) {

  $scope.aktualisieren = function(gast) {
    SignalR.emit("gastmanager.app", "Gast " + gast.id + ", " + gast.name + ", " + gast.farbe);
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
      name: "",
      farbe: null
    },
    {
      id: 1,
      name: "",
      farbe: null
    },
    {
      id: 2,
      name: "",
      farbe: null
    },
    {
      id: 3,
      name: "",
      farbe: null
    },
    {
      id: 4,
      name: "",
      farbe: null
    },
    {
      id: 5,
      name: "",
      farbe: null
    }
  ];
})

.controller('EreignisseCtrl', function($scope, SignalR, $rootScope) {
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
