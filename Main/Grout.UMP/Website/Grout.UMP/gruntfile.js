/// <binding BeforeBuild='all' ProjectOpened='watch:tasks' /> 
/* 
This file in the main entry point for defining grunt tasks and using grunt plugins. 
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409 
*/ 
module.exports = function (grunt) { 
    var local_dependency = ['minified/main.css', 'minified/dependency.min.js', 'Scripts/ext.module.js', 
                        'Scripts/global.js', 'Scripts/route.js', 
                        'Scripts/**/*.js', 
                        '!Scripts/dependency/**', '!Scripts/_references.js', '!Scripts/shared/expand-collapse/**', 
                        'minified/templates.min.js']; 
 
    var prod_dependency = ['minified/main.css', 'minified/dependency.min.js', 
                        'minified/main.min.js', 
                        'minified/templates.min.js']; 
 
    var taskName = grunt.cli.tasks[0]; 
 
    grunt.initConfig({ 
        clean: { 
            all: ["minified/*"], 
            'concat': ['minified/*.concat.js', 'minified/*.concat.min.js', 'minified/templates.js'] 
        }, 
        concat: { 
            nonminified: { 
                files: { 
                    'minified/main.concat.js': [ 
                        'Scripts/global.js', 'Scripts/route.js', 'Scripts/config/*.js',  
                        'Scripts/directive/*.js', 'Scripts/features/**/*.js', 'Scripts/shared/**/*.js', 'Scripts/service/**/*.js', 
                        'Scripts/controller/*.js'
                    ] 
                } 
            }, 
            minified: { 
                files: { 
                    'minified/dependency.min.js': [ 
                        'Scripts/dependency/jQuery/*.min.js', 
                        'Scripts/dependency/angular/angular.min.js', 
                        'Scripts/dependency/angular/*.min.js',                         
                        'Scripts/dependency/bootstrap/*.min.js', 
                        'Scripts/dependency/*.min.js'] 
                } 
            }, 
            css: { 
                files: { 
                    'minified/main.css': [ 
                        "css/bootstrap.min.css", 
                        "css/toastr.min.css", 
                        "Scripts/**/*.css", 
                        "css/*.css" 
                    ] 
                } 
            } 
        }, 
        jshint: { 
            files: ['minified/*.js'], 
            options: { 
                '-W069': false
            } 
        }, 
        uglify: { 
            main_target: { 
                files: { 
                    'minified/main.min.js': ['minified/main.concat.js'], 
                    'minified/templates.min.js': ['minified/templates.js']
                } 
            } 
        }, 
        watch: { 
            files: ['Scripts/**/*.html', 'Scripts/dependency/**', 'Views/*.html', 
                    'Content/**/*.css', 'css/**/*.css', 'index.html', 'Scripts/**/*.css'], 
            tasks: ["build-Debug"] 
        }, 
        ngtemplates: { 
            options: { 
                prefix: '/' 
            }, 
            HPDP: { 
                src: ['Views/*.html', 'Scripts/**/*.html'], 
                dest: 'minified/templates.js' 
            } 
        }, 
        injector: { 
            'dev-index': { 
                options: { 
                    destFile: 'minified/index.html'
                }, 
                files: 
                    { 'index.html': local_dependency } 
            },
 
            'prod-index': { 
                options: { 
                    destFile: 'minified/index.html' 
                }, 
                files: 
                    { 'index.html': prod_dependency } 
            }, 
            ext: { 
                options: { 
                    starttag: '<!-- @START EXT URL -->', 
                    endtag: '<!-- @END EXT URL -->', 
                    destFile: 'minified/index.html' 
                }, 
                files: { 
                    'index.html': 'minified/main.min.js' 
                } 
            } 
        }, 
        copy: { 
            main: { 
                files: [ 
                { expand: true, src: ['index.html'], dest: 'minified/' } 
                ] 
            } 
        } 
    }); 
 
    grunt.loadNpmTasks("grunt-contrib-clean"); 
    grunt.loadNpmTasks('grunt-contrib-jshint'); 
    grunt.loadNpmTasks('grunt-contrib-concat'); 
    grunt.loadNpmTasks('grunt-contrib-uglify'); 
    grunt.loadNpmTasks('grunt-contrib-watch'); 
    grunt.loadNpmTasks('grunt-angular-templates'); 
    grunt.loadNpmTasks('grunt-injector'); 
    grunt.loadNpmTasks('grunt-contrib-copy'); 
 
    grunt.registerTask('default', ['clean:all', 'copy:main', 'ngtemplates', 'concat:nonminified', 'uglify', 'concat:minified', 'concat:css', 'clean:concat', 'injector:ext']); 
    grunt.registerTask('injector-dev', ['injector:dev-index']); 
    grunt.registerTask('injector-prod', ['injector:prod-index']); 
    grunt.registerTask('build-Debug', ['default', 'injector-dev']); 
    grunt.registerTask('build-Prod', ['default', 'injector-prod']); 
}; 