<?php
@session_start(); $num = 0;
# -> Запрещяем прямой путь к файлу.
if (!defined('SECURITY_ITEFFA')) die('!');
# Корневая папка сайта
$root_path_dir = dirname(dirname(dirname(__FILE__)));
# Определяем корень сайта.
if (!defined('ROOTPATH')) define('ROOTPATH', $root_path_dir);
# Обработка запросов GET, POST
$_GET  = filter_input_array(INPUT_GET, FILTER_SANITIZE_STRING);
$_POST = filter_input_array(INPUT_POST, FILTER_SANITIZE_STRING);
?> 

Старт <hr>
