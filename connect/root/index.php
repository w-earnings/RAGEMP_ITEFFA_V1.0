<?php
# Разрешаем прямой доступ
define('SECURITY_WSCMS', true);
# Подключаем файлы конфигурации ядра
require ('ws-kernel/inc/start.php');

# Очистка запроса модуля
$module = $_GET['url'];

// Проверка наличия файла в запросе
if (preg_match('/\.php$/i', $module)) { $module_file = true;
} else {$module_file = false;}
// Если запрашивается какой-либо модуль
if (!empty($module)) {
# Проверяем существование
if (file_exists(ROOTPATH.'/iteffa-forum/'. $module) && $module_file == true){
# Подключаем модуль
require_once(ROOTPATH.'/iteffa-forum/'. $module); 
} else if (file_exists(ROOTPATH.'/iteffa-forum/'. $module) && $module_file == false) {
// Проверяем наличие главной страницы модуля
if (file_exists(ROOTPATH.'/iteffa-forum/'.$module.'/index.php')) {
# Подключаем главную страницу модуля
require_once(ROOTPATH.'/iteffa-forum/'.$module.'/index.php');
} else {
# Подключаем главную страницу сайта
require_once(ROOTPATH.'/iteffa-pages/index.php'); }
} else {
# Подключаем главную страницу сайта
require_once(ROOTPATH.'/iteffa-pages/index.php'); }
} else {
# Подключаем главную страницу или wp-pages
$module['pages'] = 'index';
if (isset($_GET['id'])) $ID = (int) $_GET['id']; else $ID = 0;
if (isset($_GET['url']) && preg_match('#^([A-z0-9\.]+)$#i', $_GET['url']))  {
if (is_file('iteffa-pages/'.$_GET['url'].'.php')) $module['pages'] = $_GET['url']; }
require 'iteffa-pages/'.$module['pages'].'.php';
}

# Подключаем ноги сайта
require ('ws-kernel/inc/exit.php');
