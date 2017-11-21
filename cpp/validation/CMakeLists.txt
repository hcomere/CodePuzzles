# Library name definition

set(LIBRARY_NAME Validation)

# Include directories

include_directories(.)

# Executable creation

add_library(
    ${LIBRARY_NAME} STATIC
    Validator.h
    Validator.cpp
)
