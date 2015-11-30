angular.module('gastmanager', ['ionic', 'ionic-color-picker', 'gastmanager.controllers', 'gastmanager.services'])

.run(function($ionicPlatform) {

  $ionicPlatform.ready(function() {
    if (window.StatusBar) {
      // org.apache.cordova.statusbar required
      StatusBar.styleDefault();
    }
  });
})

.config(function($stateProvider, $urlRouterProvider) {
  $stateProvider

  .state('app', {
    url: '/app',
    abstract: true,
    templateUrl: 'templates/menu.html',
    controller: 'AppCtrl'
  })

  .state('app.gast', {
    url: '/gast',
    views: {
      'menuContent': {
        templateUrl: 'templates/gast.html',
        controller: 'GastCtrl'
      }
    }
  })

  .state('app.ereignisse', {
    url: '/ereignisse',
    views: {
      'menuContent': {
        templateUrl: 'templates/ereignisse.html',
        controller: 'EreignisseCtrl'
      }
    }
  })

  .state('app.einstellungen', {
    url: '/einstellungen',
    views: {
      'menuContent': {
        templateUrl: 'templates/einstellungen.html',
        controller: 'EinstellungenCtrl'
      }
    }
  })
  
  $urlRouterProvider.otherwise('/app/ereignisse');
});
