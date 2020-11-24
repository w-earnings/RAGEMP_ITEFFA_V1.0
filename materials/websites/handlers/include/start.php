<?php
@session_start(); $num = 0;
# -> Запрещяем прямой путь к файлу.
if (!defined('SECURITY_ITEFFA')) die('!');
# Корневая папка сайта
$root_path_dir = dirname(dirname(dirname(__FILE__)));
# Определяем корень сайта c /.
if (!defined('H')) {define('H', $root_path_dir . '/');}
# Определяем корень сайта.
if (!defined('ROOTPATH')) define('ROOTPATH', $root_path_dir);
# Загрузка конфигурационного файла системы
if (is_file($root_path_dir . '/config.php')) {
    require($root_path_dir . '/config.php'); }
# Обработка классов с папки classes.
require(ROOTPATH.'/handlers/include/classes/autoload.php');
# Загрузка всех функций Движка
require(ROOTPATH.'/handlers/function/filemain.php');
# Site - База данных подключение
sqlsite::connect($site_host, $site_user, $site_pass, $site_name);
# Apps - База данных подключение
sqlapps::connect($apps_host, $apps_user, $apps_pass, $apps_name);
# Logs - База данных подключение
sqllogs::connect($logs_host, $logs_user, $logs_pass, $logs_name);
# Обработка запросов GET, POST
$_GET  = filter_input_array(INPUT_GET, FILTER_SANITIZE_STRING);
$_POST = filter_input_array(INPUT_POST, FILTER_SANITIZE_STRING);
