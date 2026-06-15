cmake_minimum_required(VERSION 3.27)

project(gs2lib_fixtures CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(GS2LIB_ROOT "${CMAKE_CURRENT_LIST_DIR}/../../external/gs2lib")

find_package(BZip2 REQUIRED)
find_package(ZLIB REQUIRED)

add_library(gs2lib_fixture_source STATIC
    "${GS2LIB_ROOT}/src/CCommon.cpp"
    "${GS2LIB_ROOT}/src/CEncryption.cpp"
    "${GS2LIB_ROOT}/src/CFileQueue.cpp"
    "${GS2LIB_ROOT}/src/CLog.cpp"
    "${GS2LIB_ROOT}/src/CSettings.cpp"
    "${GS2LIB_ROOT}/src/CSocket.cpp"
    "${GS2LIB_ROOT}/src/CString.cpp"
    "${GS2LIB_ROOT}/src/CTranslationManager.cpp"
    "${GS2LIB_ROOT}/src/IUtil.cpp"
    "${GS2LIB_ROOT}/src/md5.cpp"
)

target_include_directories(gs2lib_fixture_source PUBLIC "${GS2LIB_ROOT}/include")
target_link_libraries(gs2lib_fixture_source PRIVATE BZip2::BZip2 ZLIB::ZLIB)

if(WIN32)
    target_compile_definitions(gs2lib_fixture_source PRIVATE _CRT_SECURE_NO_WARNINGS STATICLIB)
    target_link_libraries(gs2lib_fixture_source PRIVATE ws2_32 wsock32 iphlpapi)
endif()

add_executable(gs2lib_filequeue_fixtures main.cpp)
target_link_libraries(gs2lib_filequeue_fixtures PRIVATE gs2lib_fixture_source)

