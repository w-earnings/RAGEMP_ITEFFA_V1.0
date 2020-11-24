<?php
# Разрешаем прямой доступ
define('SECURITY_ITEFFA', true);
# Подключаем файлы конфигурации ядра
require ('handlers/include/start.php');
# Очистка запроса модуля
$module = $_GET['iteffa'];
# Проверка наличия файла в запросе
if (preg_match('/\.php$/i', $module)) { $module_file = true;
} else {$module_file = false;}
# Если запрашивается какой-либо модуль
if (!empty($module)) {
# Проверяем существование
if (file_exists(ROOTPATH.'/handlers/plugins/'. $module) && $module_file == true){
# Подключаем модуль
require (ROOTPATH.'/handlers/plugins/'. $module); 
} else if (file_exists(ROOTPATH.'/handlers/plugins/'. $module) && $module_file == false) {
# Проверяем наличие главной страницы модуля
if (file_exists(ROOTPATH.'/handlers/plugins/'.$module.'/index.php')) {
# Подключаем главную страницу модуля
require (ROOTPATH.'/handlers/plugins/'.$module.'/index.php');
} else {
# Подключаем главную страницу сайта
require (ROOTPATH.'/handlers/plugins/index.php'); }
} else {
# Подключаем главную страницу сайта
require (ROOTPATH.'/handlers/plugins/index.php'); }
} else {
# Подключаем главную страницу или pagwes
$module['pages'] = 'index';
if (isset($_GET['id'])) $ID = (int) $_GET['id']; else $ID = 0;
if (isset($_GET['url']) && preg_match('#^([A-z0-9\.]+)$#i', $_GET['url']))  {
if (is_file(ROOTPATH.'/handlers/plugins/'.$_GET['url'].'.php')) $module['pages'] = $_GET['url']; }
# Подключаем GET страницу сайта
require (ROOTPATH.'/handlers/plugins/'.$module['pages'].'.php');
}
# Подключаем ноги сайта
require (ROOTPATH.'/handlers/include/finish.php');
