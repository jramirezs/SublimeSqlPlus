prompt Errors for file {1}:
select line + 13 || '/' || position "LINE/COL", text error
from all_errors
where owner = upper( '{0}' )
and name  = upper( '{1}' )
/