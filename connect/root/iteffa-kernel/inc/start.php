<hr> Header <hr>
<?php
@session_start(); $num = 0;
# -> Запрещяем прямой путь к файлу.
if (!defined('SECURITY_ITEFFA')) die('Прямой вызов модуля запрещен!');
# -> Корневая папка сайта
$root_path_dir = dirname(dirname(dirname(__FILE__)));
# -> Корень сайта.
if (!defined('ROOTPATH')) define('ROOTPATH', $root_path_dir);
# ->
# require(ROOTPATH.'/ws-kernel/inc/config.php');
# ->
$mysqli = new mysqli('localhost', 'root', 'usbw', 'websites');
# Проверка соединения PhpMyAdmin
if (mysqli_connect_errno()) {printf("Не удалось подключиться: %s\n", mysqli_connect_error()); exit();}
# Задаем кодеровку PhpMyAdmin
$mysqli -> set_charset("utf8");
# Регистрация сессии
if (isset($_SESSION['username'])) { $sess = $_SESSION['username'];
$users = $mysqli -> query("SELECT * FROM `users` WHERE `username` = '$sess'") -> num_rows;
# Авторизированые пользователи
if ($users > 0) {
# Задаем переменные для вывода
$user = $mysqli -> query("SELECT * FROM `users` WHERE `username` = '$sess' LIMIT 1") -> fetch_assoc();
$resource = $mysqli -> query("SELECT * FROM `resource` WHERE `user` = '".$user['id']."' LIMIT 1") -> fetch_assoc();
# ID 1 Всегда разработчик
if ($user['id'] == 1) $mysqli -> query("UPDATE `users` SET `access` = '4' WHERE `username` = '$sess'");
# Проверка таблици ресурсов
if ($resource['user'] != $user['id']) $mysqli -> query("INSERT INTO `resource` (`user`) VALUES ('".$user['id']."')");
# Записываем данные об устройстве
$mysqli -> query("UPDATE `users` SET `entry` = '".time()."' WHERE username='$sess'");
$mysqli -> query("UPDATE `users` SET `ipv4` = '".$_SERVER['REMOTE_ADDR']."' WHERE `username` = '$sess'");
$mysqli -> query("UPDATE `users` SET `agent` = '".$_SERVER['HTTP_USER_AGENT']."' WHERE `username` = '$sess'"); }}
# -> Обработка классов с папки classes.
// require(ROOTPATH.'/ws-kernel/inc/classes/autoload.php');
# -> Загрузка функций движка
// require(ROOTPATH.'/ws-kernel/inc/fnc.php');
# -> 
// require(ROOTPATH.'/ws-kernel/inc/level.php');
# -> SSL Серфитикат HTPS редирект
/*
if ($_SERVER['HTTP_X_SCHEME'] != 'https') {
header('Location: https://'.$_SERVER['HTTP_HOST'].$_SERVER['REQUEST_URI']);
exit; }
*/
# -> Редиректы с закрытых страниц (Для условия)
if ($users > 0)$redirect['error'] = '<meta http-equiv="refresh" content="0; url=/id'.$user['id'].'" />'; 
else $redirect['error'] = '<meta http-equiv="refresh" content="0; url='.$_SERVER['HTTP_HOST'].'" />';
# Обработка запросов GET, POST
$_GET  = filter_input_array(INPUT_GET, FILTER_SANITIZE_STRING);
$_POST = filter_input_array(INPUT_POST, FILTER_SANITIZE_STRING);