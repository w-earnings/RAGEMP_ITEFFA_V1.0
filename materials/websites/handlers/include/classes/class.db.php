<?php 
# -> Запрещяем прямой путь к файлу.
if (!defined('SECURITY_ITEFFA')) die('!');

class db 
{
    private static $instance;
    public static function instance()
    {
        return self::$instance;
    }

    public static function is_exsits_column($table_name, $column_name) 
    {
        $columns = array(); 
        $rows = self::select("SELECT COLUMN_NAME AS name FROM INFORMATION_SCHEMA.columns WHERE TABLE_NAME = '" . $table_name . "'"); 
        foreach($rows AS $column) {
            if(isset($column['name'])) {
                $columns[] = $column['name']; 
            }
        }

        if (in_array($column_name, $columns)) {
            return true; 
        }
        return false; 
    }
 
    public static function select($query, $hasArray = true)
    {
        $mysqli_result = self::instance()->query($query);
        if ($mysqli_result) {
            $r = array();

            while ($row = $mysqli_result->fetch_object()) {
                $r[] = $hasArray ? (array) $row : $row;
            }
 
            return $r;
        }
 
        return array();
    }

    public static function fetch($query, $hasArray = true)
    {
        $mysqli_result = self::instance()->query($query);
        if ($mysqli_result) {
            $row = $mysqli_result->fetch_assoc();
            if ($row) {
                if ($hasArray) {
                    return (array) $row;
                }
                else {
                    return $row;
                }
            }
        }
 
        return array();
    }

    public static function get_row($query)
    {
        $rows = self::select($query);
        return array_map(function ($row) {
            return array_shift($row);
        }, $rows);
    }

    public static function count($query)
    {
        $count = 0;
        $mysqli_result = self::instance()->query($query);
        if($mysqli_result) {
            $row = $mysqli_result->fetch_row();
            if ($row) {
                $count = (int) $row[0];
            }
        }
 
        return $count;
    }

    public static function get_type($str) {
        if (ctype_digit((string) $str)) {
            return ($str <= PHP_INT_MAX ? 'i' : 's');
        }

        if (is_numeric($str)) {
            return 'd'; 
        }

        return 's'; 
    }

    public static function update($table, $query, $where = array(), $limit = NULL) 
    {
        $query = self::get_construct_query_update($query);
        $where = self::get_construct_query_where($table, $where);

        $sql = 'UPDATE `' . $table . '` SET ' . join(',', $query['query_keys']) . ' WHERE 1=1 ' . $where . ' ' . $limit;

        if ($stmt = self::instance()->prepare($sql)) {
            $types = ''; 
            foreach($query['query_params'] AS $value) {
                $types .= self::get_type($value); 
                $bind_name = $value;
                $$bind_name = $value;
                $params[] = &$$bind_name;
            }

            call_user_func_array(array($stmt, 'bind_param'), array_merge(array($types), $params));

            if ($stmt->execute()) {
                $stmt->close();
                return true;
            }
        }

        return false; 
    }

    public static function delete($table, $where = array(), $limit = '') 
    {
        $where = self::get_construct_query_where($table, $where);

        $sql = 'DELETE FROM `' . $table . '` WHERE 1=1 ' . $where . ' ' . $limit;

        if ($stmt = self::instance()->prepare($sql)) {
            if ($stmt->execute()) {
                $stmt->close();
                return true;
            }
        }

        return false; 
    }

    public static function insert($table, $query) 
    {
        $query = self::get_construct_query_insert($query);
        $sql = "INSERT INTO `" . $table . "` (" . join(',', $query['query_keys']) . ") VALUES(" . join(',', $query['query_values']) . ")";

        if ($stmt = self::instance()->prepare($sql)) {
            $types = ''; 
            foreach($query['query_params'] AS $value) {
                $types .= self::get_type($value); 
                $bind_name = $value;
                $$bind_name = $value;
                $params[] = &$$bind_name;
            }

            call_user_func_array(array($stmt, 'bind_param'), array_merge(array($types), $params));

            if ($stmt->execute()) {
                $stmt->close();
                return true;
            }
        }

        return false; 
    }

    public static function query($query)
    {
        return self::instance()->query($query);
    }
 
    public static function error()
    {
        return self::instance()->error;
    }
 
    public static function insert_id()
    {
        return self::instance()->insert_id;
    }

	
    public static function connect($host, $user, $pass, $name)
    {
    	try {
    		$mysqli = new mysqli($host, $user, $pass, $name);
    	}
    	
        catch(Exception $e) {
	        if ($mysqli->connect_errno) { 
	            ds_die(sprintf("Не удалось подключиться к базе данных: %s", $mysqli->connect_errno));
	        }
        }
        
		# iTeffa | Проверка соединения - Временное решения
		if (mysqli_connect_errno()) {
			printf("Не удалось подключиться: %s\n", mysqli_connect_error()); 
		    die();
		}
		
        db::$instance = $mysqli;

    }

	
    public static function esc($str) {
        return self::instance()->real_escape_string($str); 
    }

    public static function get_construct_query_insert($array) 
    {
        $construct = array(
            'query_keys' => array(), 
            'query_values' => array(), 
            'query_params' => array(), 
        ); 

        foreach($array AS $key => $value) {
            array_push($construct['query_keys'], '`' . $key . '`');
            array_push($construct['query_values'], '?');
            array_push($construct['query_params'], $value);
        }

        return $construct;
    }

    public static function get_construct_query_update($array) 
    {
        $construct = array(
            'query_keys'   => array(), 
            'query_params' => array(), 
        ); 

        foreach($array AS $key => $value) {
            array_push($construct['query_keys'], "`" . $key . "` = ?");
            array_push($construct['query_params'], $value);
        }

        return $construct;
    }

    public static function get_construct_query_where($table, $args, $before = ' AND ') 
    {
        $sql = array(); 
        $str = " " . $before . " (";

        foreach($args AS $key => $value) {
            if (isset($value['field'])) {
                if (!isset($value['operator'])) {
                    $value['operator'] = '=';
                }
                $sql[] = $table . '.' . $value['field'] . ' ' . strtoupper($value['operator']) . ' \'' . self::esc($value['value']) . '\'';
            }  
            
            elseif (isset($value['relation'])) {
                $sql[] = self::get_construct_query_where($table, $value); 
            }

            elseif (!is_array($value)) {
                $sql[] = $table . '.' . $key . ' = \'' . self::esc($value) . '\'';
            }
        }

        if (!isset($args['relation'])) {
            $args['relation'] = 'AND';
        }

        if (!$sql) {
            return '';
        }
        
        $str .= implode(' ' . strtoupper($args['relation']) . ' ', $sql); 
        $str .= ")";
       
        return $str; 
    }
    
}