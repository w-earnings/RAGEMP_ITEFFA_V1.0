<?php 
# -> Запрещяем прямой путь к файлу.
if (!defined('SECURITY_ITEFFA')) die('!');
# PHP_VERSION >= PHP 5 Автозагрузка классов
function __autoload($class) {iteffa_autoload($class);}
