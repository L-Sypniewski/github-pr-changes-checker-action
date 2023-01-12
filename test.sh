#!/usr/bin/env sh

# str='[".github","Core","DependencyInjection","LoggingSerilog"]'

# value=$(echo "$str" | jq '. | index("DependencyInjection")')
# echo "XXX: $value"
# if [ "$value" = "null" ]; then result="NULL"; else result="Not NULL"; fi
# echo "Result: $result"


   UPDATED_PROJECTS='[".github","Core","DependencyInjection","LoggingSerilog"]'
   INDEX_OF_PROJECT=$(echo "$UPDATED_PROJECTS" | jq '. | index("DependencyInjectionAutofac")')
#    INDEX_OF_PROJECT=$(echo "$UPDATED_PROJECTS" | jq '. | index(DependencyInjectionAutofac)')
  if [ "$INDEX_OF_PROJECT" = "null" ]; then PROJECT_WAS_MODIFIED=false; else PROJECT_WAS_MODIFIED=true; fi
  # echo PROJECT_WAS_MODIFIED='[".github","Core","DependencyInjection","LoggingSerilog"]' >> $GITHUB_ENV
#   echo PROJECT_WAS_MODIFIED >> $GITHUB_ENV
#   echo UPDATED_PR 



  # docker-compose -f docker-compose.debug.yml -f docker-compose.yml up  --build