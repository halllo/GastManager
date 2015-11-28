angular.module('gastmanager.controllers', [])

.controller('AppCtrl', function($scope, $ionicModal, $timeout) {

})

.controller('EinstellungenCtrl', function($scope) {

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

  $scope.newEvent = {
    clientName: "",
    eventMessage: ""
  };

  $scope.emit = function() {
    SignalR.emit($scope.newEvent.clientName, $scope.newEvent.eventMessage);
    $scope.newEvent.eventMessage = "";
  };
})
