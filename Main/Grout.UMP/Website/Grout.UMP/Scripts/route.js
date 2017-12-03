(function () { 
    angular.module('HPDP').config(['$routeProvider', routeConfig]); 
 
    function routeConfig($routeProvider) { 
        $routeProvider.caseInsensitiveMatch = true; 
 
        var resolver = { 
            access: ['accessService', function (accessService) { 
                return accessService.getAccess(); 
                //return true; 
            }] 
        }; 
 
        //With Project 
        $routeProvider.when('/Project/:e2eId/:displayScreen?', { 
            templateUrl: function (params) { 
                switch (angular.lowercase(params.displayScreen)) { 
                    case 'configuration': return '/Scripts/features/configure-project-tile-nav/configuration.html'; 
                    case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                    case 'effortandschedule': return '/Scripts/features/effort-schedule-project-tile-nav/nav.html'; 
                    case 'agileroadmap': return '/Views/ProjectProgressView.html'; 
                    case 'managebacklog': return '/Views/ManageBacklogView.html'; 
                    case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                    case 'managechange': return '/Views/ManageChangeView.html'; 
                    case 'manageresourcing': return '/Views/ResourceDetailsView.html'; 
                    case 'defect': return '/Views/DefectsView.html'; 
                    case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                    default: return '/Views/ProjectView.html'; 
                } 
            }, 
            resolve: resolver 
        }) 
           
           //with Project/cms 
          .when('/Project/:e2eId/CMS/:cmsId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'configuration': return '/Scripts/features/configure-project-tile-nav/configuration.html'; 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'effortandschedule': return '/Scripts/features/effort-schedule-project-tile-nav/nav.html'; 
                      case 'agileroadmap': return '/Views/ProjectProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'managechange': return '/Views/ManageChangeView.html'; 
                      case 'manageresourcing': return '/Views/ResourceDetailsView.html'; 
                      case 'defect': return '/Views/DefectsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ProjectView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
          // With Project/Cms/Team 
          .when('/Project/:e2eId/CMS/:cmsId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'agileroadmap': return '/Views/ProjectProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'impediment': return '/Views/ImpedimentsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ProjectView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
          //With  Project/ Team 
          .when('/Project/:e2eId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'agileroadmap': return '/Views/ProjectProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'impediment': return '/Views/ImpedimentsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ProjectView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
  //With  Project/ Release 
          .when('/Project/:e2eId/Release/:releaseId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'managesprint': return '/Scripts/features/manage-sprint-tile-nav/manage-sprint-tile-nav.html'; 
                      case 'effortandschedule': return '/Scripts/features/effort-schedule-project-tile-nav/nav.html'; 
                      case 'agileroadmap': return '/Views/ReleaseProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogReleaseView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'managechange': return '/Views/ManageChangeView.html'; 
                      case 'impediment': return '/Views/ImpedimentsView.html'; 
                      case 'manageresourcing': return '/Views/ManageResourcingReleaseView.html'; 
                      case 'defect': return '/Views/DefectsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ReleaseView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
   //With  Project/ Release/ Team 
          .when('/Project/:e2eId/Release/:releaseId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'managesprint': return '/Scripts/features/manage-sprint-tile-nav/manage-sprint-tile-nav.html'; 
                      case 'agileroadmap': return '/Views/ReleaseProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogReleaseView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'impediment': return '/Views/ImpedimentsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ReleaseView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
          //With  Project/ Team/ Release 
          .when('/Project/:e2eId/Team/:teamId/Release/:releaseId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'requirementanalysisdetail': return '/Views/RequirementAnalysisView.html'; 
                      case 'managesprint': return '/Scripts/features/manage-sprint-tile-nav/manage-sprint-tile-nav.html'; 
                      case 'agileroadmap': return '/Views/ReleaseProgressView.html'; 
                      case 'managebacklog': return '/Views/ManageBacklogReleaseView.html'; 
                      case 'requirementanalysis': return '/Views/AnalyseStoriesView.html'; 
                      case 'impediment': return '/Views/ImpedimentsView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/ReleaseView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
  //With  Project/ Sprint 
          .when('/Project/:e2eId/Sprint/:sprintId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
   //With  Project/ Sprint/ Team 
          .when('/Project/:e2eId/Sprint/:sprintId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
          //With  Project/ Team/ Sprint 
          .when('/Project/:e2eId/Team/:teamId/Sprint/:sprintId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
//With  Project/ Release/ Sprint 
          .when('/Project/:e2eId/Release/:releaseId/Sprint/:sprintId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
          //With  Project/ Sprint/ Release 
          .when('/Project/:e2eId/Sprint/:sprintId/Release/:releaseId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
   //With  Project/ Sprint/ Release/ Team 
          .when('/Project/:e2eId/Sprint/:sprintId/Release/:releaseId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
  //With  Project/ Sprint/ Team/ Release 
          .when('/Project/:e2eId/Sprint/:sprintId/Team/:teamId/Release/:releaseId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
  //With  Project/ Release/ Sprint/ Team 
          .when('/Project/:e2eId/Release/:releaseId/Sprint/:sprintId/Team/:teamId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
 
  //With  Project/ Team/ Release/ Sprint 
          .when('/Project/:e2eId/Team/:teamId/Release/:releaseId/Sprint/:sprintId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      case 'impediment': return '/Views/SprintImpedimentsView.html'; 
                      case 'sprintallocation': return '/Views/SprintAllocationView.html'; 
                      case 'sprintbacklogvolatility': return '/Views/SprintBacklogVolatilityView.html'; 
                      case 'sprintquality': return '/Views/SprintQualityView.html'; 
                      case 'sprintbacklog': return '/Views/SprintBacklogView.html'; 
                      case 'sprintroadmap': return '/Views/SprintRoadmapView.html'; 
                      case 'scrumassistant': return '/Scripts/features/scrum-assistant-bots/scrum-assistant-bots.html'; 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
  //With  Project/ Release/ Team/ Sprint 
          .when('/Project/:e2eId/Release/:releaseId/Team/:teamId/Sprint/:sprintId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
  //With  Project/ Team/ Sprint/ Release 
          .when('/Project/:e2eId/Team/:teamId/Sprint/:sprintId/Release/:releaseId/:displayScreen?', { 
              templateUrl: function (params) { 
                  switch (angular.lowercase(params.displayScreen)) { 
                      default: return '/Views/SprintView.html'; 
                  } 
              }, 
              resolve: resolver 
          }) 
         .when('/file/:e2eId/:releaseId?', { 
             templateUrl: '/Views/RequirementAnalysis.html', 
             resolve: resolver 
         }) 
 
        .when('/Report/:reportId/ReportAssistantFileGenerator', { 
            templateUrl: '/Views/PreviewReport.html', 
            resolve: resolver 
        }) 
.otherwise({ 
    templateUrl: '/Views/AccessDenied.html', 
    controller: ['$rootScope', '$scope', '$location', function ($rootScope, $scope, $location) { 
        $rootScope.isPageNotDefined = true; 
        $scope.isServiceError = false; 
        if ($location.url() == '/error.html') { 
            $scope.isServiceError = true; 
        } 
    }] 
}); 
    } 
})(); 